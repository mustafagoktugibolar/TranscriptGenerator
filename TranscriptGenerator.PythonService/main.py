from fastapi import FastAPI, HTTPException, UploadFile, File, Form
from pydantic import BaseModel
import os
import shutil
import uuid
from service_logic import download_youtube_audio, transcribe_audio, get_model

app = FastAPI()

class YoutubeRequest(BaseModel):
    url: str
    model: str = "base"

@app.on_event("startup")
async def startup_event():
    """Preload Whisper model on startup to avoid cold start delays"""
    print("🚀 Preloading Whisper model...")
    try:
        model = get_model("base")
        print(f"✅ Whisper model loaded successfully!")
        print(f"   Model is ready for transcription requests")
    except Exception as e:
        print(f"⚠️ Warning: Could not preload model: {e}")
        print(f"   Model will be loaded on first request")

@app.post("/transcribe_youtube")
def transcribe_youtube(request: YoutubeRequest):
    try:
        # Download
        mp3_path = download_youtube_audio(request.url)
        
        try:
            # Transcribe
            text = transcribe_audio(mp3_path, request.model)
            return {"text": text}
        finally:
            # Cleanup
            if os.path.exists(mp3_path):
                os.remove(mp3_path)
                
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.post("/transcribe_file")
async def transcribe_file(file: UploadFile = File(...), model: str = Form("base")):
    temp_filename = f"upload_{uuid.uuid4().hex}_{file.filename}"
    temp_path = os.path.join("/tmp", temp_filename)
    
    try:
        with open(temp_path, "wb") as buffer:
            shutil.copyfileobj(file.file, buffer)
            
        text = transcribe_audio(temp_path, model)
        return {"text": text}
        
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        if os.path.exists(temp_path):
            os.remove(temp_path)

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)
