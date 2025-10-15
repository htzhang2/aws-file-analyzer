import React, { useState } from "react";
import axios from "axios";

export default function App() {
  const [file, setFile] = useState(null);
  const [uploadSuccess, setUploadSuccess] = useState(false);
  const [analyzeResult, setAnalyzeResult] = useState(null);
  const [loading, setLoading] = useState(false);
  const [analyzing, setAnalyzing] = useState(false);
  const [message, setMessage] = useState("");
  const [fileUrl, setFileUrl] = useState("");

  const handleFileChange = (e) => {
    setFile(e.target.files[0]);
    setUploadSuccess(false);
    setAnalyzeResult(null);
  };

  const handleUpload = async () => {
    if (!file) return;
    setLoading(true);
    setMessage("");

    try {
      const formData = new FormData();
      formData.append("file", file);

      // Replace with your API endpoint
      const res = await axios.post("https://localhost:5000/OpenAIAws/AwsFileUpload", formData, {
        headers: { "Content-Type": "multipart/form-data" },
      });

      if (res.status === 200) {
        setFileUrl(res.data.fileUrl);
        setUploadSuccess(true);
        setMessage("File uploaded successfully ✅");
      }
    } catch (err) {
      console.error(err);
      setMessage("Upload failed ❌");
    } finally {
      setLoading(false);
    }
  };

  const handleAnalyze = async () => {
    setAnalyzing(true);
    setMessage("");
    try {
      // Replace with your API endpoint
      const res = await axios.post("https://localhost:5000/OpenAIAws/OpenAISummary", {
        fileUrl: fileUrl, // or S3 key if your backend needs it
      });

      if (res.status === 200) {
        setAnalyzeResult(res.data);
        setMessage("Analysis complete ✅");
      }
    } catch (err) {
      console.error(err);
      setMessage("Analysis failed ❌");
    } finally {
      setAnalyzing(false);
    }
  };

  return (
    <div className="flex flex-col items-center justify-center min-h-screen bg-gray-100 p-6">
      <div className="bg-white shadow-md rounded-2xl p-6 w-full max-w-md">
        <h1 className="text-xl font-bold mb-4">AI File Analyzer</h1>

        <input
          type="file"
          onChange={handleFileChange}
          className="mb-4 block w-full text-sm text-gray-700"
        />

        <button
          onClick={handleUpload}
          disabled={!file || loading}
          className={`w-full py-2 px-4 rounded-lg mb-2 ${
            !file || loading
              ? "bg-gray-400 text-white cursor-not-allowed"
              : "bg-blue-600 hover:bg-blue-700 text-white"
          }`}
        >
          {loading ? "Uploading..." : "Upload"}
        </button>

        <button
          onClick={handleAnalyze}
          disabled={!uploadSuccess || loading}
          className={`w-full py-2 px-4 rounded-lg mb-2 ${
            !uploadSuccess || loading
              ? "bg-gray-400 text-white cursor-not-allowed"
              : "bg-green-600 hover:bg-green-700 text-white"
          }`}
        >
          {analyzing ? "Analyzing..." : "Analyze"}
        </button>

        {message && <p className="mt-2 text-center">{message}</p>}

        {analyzeResult && (
          <div className="mt-4 p-3 bg-gray-100 rounded-lg">
            <h2 className="font-semibold">Result:</h2>
            <pre className="text-sm whitespace-pre-wrap">
              {JSON.stringify(analyzeResult, null, 2)}
            </pre>
          </div>
        )}
      </div>
    </div>
  );
}
