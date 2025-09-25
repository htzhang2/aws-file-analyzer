#  File Uploader and Analyzer
Imagine your desktop stored many old photos or text files, you forget where photos were taken, or what is the summary for text file.  You want to upload these files to AWS S3, then use OpenAI api to analyze. You need an app/service to do this.

This repro includes both backend service(written in .Net Core 8) and Frontend UI (written in React)

This project was on github https://github.com/htzhang2/aws-file-analyzer.

# Setup

. OpenAI account API key (put in backend appsettings.development.json for local test)
. AWS free account with IAM user with S3 permission 
. Create a AWS S3 bucket from AWS console, update backend controller file
. AWS CLI command: aws configure (for aws_access_key_id / aws_secret_access_key)

# Future Plan 

. Deploy backend service to AWS
. Save analysis result to DynamoDB
. More file type support (etc. PDF/audio/video)