import React, { useState } from "react";

export default function AiVoicePlayer({ analysisText }) {
  const [speaking, setSpeaking] = useState(false);

  const handleSpeak = () => {
    if (!analysisText) return;
    let speechText = analysisText;

    if (analysisText.caption) {
        speechText = analysisText.caption;
    }
    const utterance = new SpeechSynthesisUtterance(speechText);
    utterance.lang = "en-US";
    utterance.rate = 1.0;
    utterance.pitch = 1.0;

    utterance.onstart = () => setSpeaking(true);
    utterance.onend = () => setSpeaking(false);

    window.speechSynthesis.speak(utterance);
  };

  const handleStop = () => {
    window.speechSynthesis.cancel();
    setSpeaking(false);
  };

  return (
    <div className="flex items-center gap-2">
      {!speaking ? (
        <button
          onClick={handleSpeak}
          className="bg-blue-500 text-white px-3 py-1 rounded-lg shadow"
        >
          🔊 Play AI Result
        </button>
      ) : (
        <button
          onClick={handleStop}
          className="bg-red-500 text-white px-3 py-1 rounded-lg shadow"
        >
          ⏹ Stop
        </button>
      )}
    </div>
  );
}