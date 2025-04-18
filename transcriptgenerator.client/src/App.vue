<template>
  <div class="max-w-2xl mx-auto p-6">
    <h1 class="text-3xl font-bold mb-4">🎧 Transcribe Generator</h1>

    <div class="mb-4">
      <label class="block mb-2 font-medium">Video Kaynağı Türü</label>
      <select v-model="inputType" class="w-full border rounded p-2">
        <option value="youtube">YouTube Linki</option>
        <option value="file">MP3 / MP4 Dosyası</option>
      </select>
    </div>

    <div class="mb-4" v-if="inputType === 'youtube'">
      <label class="block mb-2 font-medium">YouTube Linki</label>
      <input v-model="youtubeLink" type="text" placeholder="https://youtube.com/..." class="w-full border rounded p-2" />
    </div>

    <div class="mb-4" v-else>
      <label class="block mb-2 font-medium">Ses / Video Dosyası</label>
      <input type="file" @change="handleFileUpload" accept=".mp3,.mp4" class="w-full border rounded p-2" />
    </div>

    <button @click="submit" class="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700">
      Gönder
    </button>

    <div class="mt-6">
      <label class="block mb-2 font-medium">Transcript</label>
      <textarea v-model="transcript" readonly class="w-full border rounded p-2 h-48"></textarea>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const inputType = ref('youtube')
const youtubeLink = ref('')
const selectedFile = ref(null)
const transcript = ref('')

const handleFileUpload = (event) => {
  selectedFile.value = event.target.files[0]
}

const submit = async () => {
  if (inputType.value === 'youtube') {
    // YouTube linki ile API çağrısı yapılacak
    // Örnek: axios.post('/api/transcribe/youtube', { url: youtubeLink.value })
  } else {
    // Dosya ile API çağrısı yapılacak
    // Örnek: FormData kullanarak dosya gönderimi
  }

  // API'den gelen transcript verisi transcript.value'ya atanacak
}
</script>
