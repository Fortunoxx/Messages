using Scalar.AspNetCore;
using AutoBogus;
using AutoBogus.Conventions;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Messages API V1");
    });
    app.MapScalarApiReference(options =>
    {
        options.
            WithTitle("Messages API").
            WithTheme(ScalarTheme.Solarized).
            WithDefaultHttpClient(ScalarTarget.PowerShell, ScalarClient.Curl);
    });
}

app.UseHttpsRedirection();

AutoFaker.Configure(builder =>
{
    builder.WithLocale("de");
    builder.WithConventions(cfg =>
    {
        cfg.StreetName.Aliases("Street", "Strasse", "Straße");
        cfg.PhoneNumber.Aliases("Phone", "Mobile", "Tel", "Telefon", "Fax", "Mobil", "Rufnummer");
        cfg.ZipCode.Aliases("PostalCode", "PLZ", "Postleitzahl");
    });
});

List<string> mediaTypeNames = [
    MediaTypeNames.Application.Pdf,
    MediaTypeNames.Image.Jpeg,
    MediaTypeNames.Image.Png,
    MediaTypeNames.Image.Gif,
    MediaTypeNames.Image.Tiff,
    MediaTypeNames.Image.Bmp,
    MediaTypeNames.Application.Zip,
    MediaTypeNames.Application.Octet,
    MediaTypeNames.Application.Rtf,
    MediaTypeNames.Application.Xml,
    MediaTypeNames.Application.Json,
];

// TODO: use a lighweight attachment DTO for the message details
app.MapGet("/messages/outbox/{sender:guid}", (Guid sender) =>
{
    var messageid = Guid.NewGuid();

    var attachments = new AutoFaker<AttachmentLight>()
        .RuleFor(a => a.Id, Guid.NewGuid)
        .RuleFor(a => a.MessageId, messageid)
        .RuleFor(a => a.FileName, f => f.System.FileName())
        .RuleFor(a => a.ContentType, f => f.PickRandom(mediaTypeNames))
        .RuleFor(a => a.Size, f => f.Random.Number(1000, 1000000))
        .Generate(3);

    var faker = new AutoFaker<Message>()
        .RuleFor(m => m.Id, messageid)
        .RuleFor(m => m.Sender, sender)
        .RuleFor(m => m.Receiver, Guid.NewGuid)
        .RuleFor(m => m.Title, f => f.Lorem.Sentence())
        .RuleFor(m => m.Content, f => f.Lorem.Paragraph())
        .RuleFor(m => m.SentAt, f => f.Date.Past(1))
        .RuleFor(m => m.IsRead, f => f.Random.Bool())
        .RuleFor(m => m.Attachments, attachments);

    var messages = faker.Generate(3);

    return messages;
})
.WithName("GetMessagesOutbox");

// TODO: use a lighweight attachment DTO for the message details
app.MapGet("/messages/inbox/{receiver:guid}", (Guid receiver) =>
{
    var messageid = Guid.NewGuid();

    var attachments = new AutoFaker<AttachmentLight>()
        .RuleFor(a => a.Id, Guid.NewGuid)
        .RuleFor(a => a.MessageId, messageid)
        .RuleFor(a => a.FileName, f => f.System.FileName())
        .RuleFor(a => a.ContentType, f => f.PickRandom(mediaTypeNames))
        .RuleFor(a => a.Size, f => f.Random.Number(1000, 1000000))
        .Generate(3);

    var faker = new AutoFaker<Message>()
        .RuleFor(m => m.Id, messageid)
        .RuleFor(m => m.Sender, Guid.NewGuid)
        .RuleFor(m => m.Receiver, receiver)
        .RuleFor(m => m.Title, f => f.Lorem.Sentence())
        .RuleFor(m => m.Content, f => f.Lorem.Paragraph())
        .RuleFor(m => m.SentAt, f => f.Date.Past(1))
        .RuleFor(m => m.IsRead, f => f.Random.Bool())
        .RuleFor(m => m.Attachments, attachments);

    var messages = faker.Generate(3);

    return messages;
})
.WithName("GetMessagesInbox");

app.MapGet("messages/{messageId:guid}", (Guid messageId) =>
{
    var messageid = Guid.NewGuid();

    var attachments = new AutoFaker<AttachmentLight>()
        .RuleFor(a => a.Id, Guid.NewGuid)
        .RuleFor(a => a.MessageId, messageId)
        .RuleFor(a => a.FileName, f => f.System.FileName())
        .RuleFor(a => a.ContentType, f => f.PickRandom(mediaTypeNames))
        .RuleFor(a => a.Size, f => f.Random.Number(1000, 1000000))
        .Generate(3);

    var faker = new AutoFaker<Message>()
        .RuleFor(m => m.Id, messageId)
        .RuleFor(m => m.Sender, Guid.NewGuid)
        .RuleFor(m => m.Receiver, Guid.NewGuid)
        .RuleFor(m => m.Title, f => f.Lorem.Sentence())
        .RuleFor(m => m.Content, f => f.Lorem.Paragraph())
        .RuleFor(m => m.SentAt, f => f.Date.Past(1))
        .RuleFor(m => m.IsRead, f => f.Random.Bool())
        .RuleFor(m => m.Attachments, attachments);

    var message = faker.Generate();

    return message;
})
.WithName("GetMessageDetails");

app.MapPatch("messages/{messageId:guid}/{isRead:bool}", (Guid messageId, bool isRead) =>
{
    // Update the message with the new values
    // message.IsRead = isRead;
    // Save the changes to the database or any other storage
    // context.SaveChangesAsync();
    return Results.NoContent();
})
.WithName("PatchMessageReadStatus");

app.MapPost("messages/{messageId:guid}/attachment", (Guid messageId, [FromBody] AttachmentInsert attachment) =>
{
    // create an attachment for the message with the given messageId
    return Results.CreatedAtRoute("GetMessageAttachmentDetails", new { attachmentId = Guid.NewGuid() }, new Attachment(
        Id: Guid.NewGuid(),
        MessageId: messageId,
        FileName: attachment.FileName,
        ContentType: attachment.ContentType,
        Size: attachment.Data.Length,
        Data: attachment.Data
    ));
})
.WithName("PostMessageAttachments");

app.MapGet("attachments/{attachmentId:guid}", (Guid attachmentId) =>
{
    // get the attachment with the given attachmentId
    var attachment = new AutoFaker<Attachment>()
        .RuleFor(a => a.Id, attachmentId)
        .RuleFor(a => a.MessageId, Guid.NewGuid)
        .RuleFor(a => a.FileName, f => f.System.FileName())
        .RuleFor(a => a.ContentType, f => f.PickRandom(mediaTypeNames))
        .RuleFor(a => a.Size, f => f.Random.Number(1000, 1000000))
        .RuleFor(a => a.Data, f => f.Random.Bytes(1000))
        .Generate();

    return attachment;
})
.WithName("GetMessageAttachmentDetails");

app.Run();


record Message(Guid Sender, Guid Receiver, string Title, string Content, DateTime SentAt, bool IsRead)
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public IEnumerable<AttachmentLight> Attachments { get; set; } = [];
}

record Attachment(Guid Id, Guid MessageId, string FileName, string ContentType, long Size, byte[] Data) : AttachmentLight(Id, MessageId, FileName, ContentType, Size) {}

record AttachmentLight(Guid Id, Guid MessageId, string FileName, string ContentType, long Size)
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

record AttachmentInsert(Guid MessageId, string FileName, string ContentType, byte[] Data) { }