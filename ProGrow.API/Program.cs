using ProGrow.API.Models;
using ProGrow.API.Services.Implementations.Authentication;
using ProGrow.API.Services.Implementations.Community;
using ProGrow.API.Services.Interfaces.Authentication;
using ProGrow.API.Services.Interfaces.Community;
using ProGrow.API.Services.Interfaces.Admin;
using ProGrow.API.Services.Implementations.Admin;
using ProGrow.API.Services.Implementations.Finance;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SemanticKernel;
using System.Text;
using ProGrow.API.Services.Implementations.AI;
using ProGrow.API.Services.Interfaces.Finance;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddKernel();
builder.Services.AddGoogleAIGeminiChatCompletion(builder.Configuration["AI:Gemini:Model"],
builder.Configuration["AI:Gemini:Apikey"]);

var jwtSettings = builder.Configuration.GetSection("JWT");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

    // For external login (Google)
    options.DefaultSignInScheme = "External";
})
    .AddCookie("External") // temporary cookie for Google OAuth
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]))
        };
    })
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
        options.CallbackPath = "/api/auth/google-callback";
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RecruiterOnly", policy =>
        policy.RequireClaim("AuthorType", "Recruiter"));

    options.AddPolicy("JobSeekerOnly", policy =>
        policy.RequireClaim("AuthorType", "JobSeeker"));

    options.AddPolicy("AdminOnly", policy =>
        policy.RequireClaim("AuthorType", "Admin"));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IPostLikeService, PostLikeService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IPostSaveService, PostSaveService>();
builder.Services.AddScoped<IJobLikeService, JobLikeService>();
builder.Services.AddScoped<IJobSaveService, JobSaveService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<ICommunityService, CommunityService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<ISalaryCalculationService, SalaryCalculationService>();
builder.Services.AddScoped<FileParsingService>();
builder.Services.AddScoped<LanguageDetectionService>();
builder.Services.AddScoped<CvProcessingService>();
builder.Services.AddScoped<GeminiCvEvaluationService>();
builder.Services.AddScoped<CareerChatService>();

var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.UseCors("AllowReact");

app.MapControllers();

app.Run();
