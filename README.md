#  AWS File Analyzer
Imagine your desktop stored many old photos or text files, you forget where photos were taken, or what is the summary for text file. You want to upload these files to AWS S3, then use OpenAI api to analyze, display analysis result in structured format, then play audio summary.

A full-stack project with .NET 8 API + React UI.  
Login, uploads local files to AWS S3, save file meta inforamtion into Azure SQL db, generate a presigned url and analyzes url with OpenAI API.

## 🚀 Demo Screenshots

### Upload UI
![Upload UI](./screenshots/ui-upload-success.png)
![Analyze UI](./screenshots/ui-analysis-result.png)

### API Response
![API Upload](./screenshots/api-upload-jpg.png)
![API Summary](./screenshots/api-analyze-image.png)

## 🚀 Tech Stack
- Backend: .NET 8 Web API (C#), EF Core
- Frontend: React + Axios
- Cloud: AWS S3, Azure SQL Db
- OpenAI: API
- Authentication: Jwt token

## 📦 How to Run Locally
### Backend
```bash
cd backend
update OpenAI:ApiKey, ConnectionStrings:DefaultConnection in appsettings.development.json
update S3BucketName in OpenAwsController.cs
aws configure (for aws_access_key_id / aws_secret_access_key)
dotnet restore
dotnet ef database update
dotnet run

### Frontend

cd frontend
npm install
npm start


## Future Plan 

Deploy backend service to AWS
Photo Ablum based on analysis result
More file type support (etc. PDF/audio/video)