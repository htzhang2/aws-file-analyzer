using Amazon.S3;
using OpenAI.Chat;
using OpenAiChat.Services;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

string modelName = builder.Configuration["OpenAI:ModelName"];
string ApiKey = builder.Configuration["OpenAI:ApiKey"];

ChatClient chatClient = new(
    model: modelName,
    apiKey: ApiKey
);


// Add services to the container.
builder.Services.AddSingleton(chatClient);

builder.Services.AddHttpClient();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(options =>
{
    // Find the XML file path
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

    // Instruct Swashbuckle to include XML comments
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

// Configure AWS services from appsettings.json
builder.Services.AddAWSService<IAmazonS3>();

builder.Services.AddSingleton<ITextService, TextService>();
builder.Services.AddSingleton<IImageService, ImageService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowReactApp");
app.UseAuthorization();

app.MapControllers();

app.Run();
