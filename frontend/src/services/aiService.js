import { forexKnowledge } from '../data/forexKnowledge';

// Key for saving chat history to localStorage
const MEMORY_KEY = 'white_code_ai_memory';

// Initialize memory with the system prompt
export function getChatMemory() {
  const stored = localStorage.getItem(MEMORY_KEY);
  if (stored) {
    try {
      return JSON.parse(stored);
    } catch (e) {
      console.error('Error parsing AI memory', e);
    }
  }
  return [
    { role: 'system', content: forexKnowledge }
  ];
}

export function saveChatMemory(memory) {
  localStorage.setItem(MEMORY_KEY, JSON.stringify(memory));
}

export function clearChatMemory() {
  localStorage.removeItem(MEMORY_KEY);
}

// Main function to send message to AI
export async function sendMessageToAI(userMessage) {
  const memory = getChatMemory();
  
  // Add new user message to memory
  const newMemory = [...memory, { role: 'user', content: userMessage }];
  saveChatMemory(newMemory);

  const apiKey = import.meta.env.VITE_OPENAI_API_KEY;

  if (!apiKey) {
    // Mock response if no API key is provided
    return new Promise((resolve) => {
      setTimeout(() => {
        const mockReply = "هذا رد تجريبي (Mock) لأن مفتاح VITE_OPENAI_API_KEY غير مضاف في ملف .env.\n\n" +
          "أنا أتذكر أنك قلت سابقاً أموراً في هذه المحادثة، وعدد الرسائل حتى الآن هو: " + newMemory.length;
        
        const finalMemory = [...newMemory, { role: 'assistant', content: mockReply }];
        saveChatMemory(finalMemory);
        resolve({ reply: mockReply, memory: finalMemory });
      }, 1500);
    });
  }

  // Real OpenAI Call
  try {
    const response = await fetch('https://api.openai.com/v1/chat/completions', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${apiKey}`
      },
      body: JSON.stringify({
        model: 'gpt-4o-mini', // or gpt-3.5-turbo
        messages: newMemory,
        temperature: 0.7,
      })
    });

    if (!response.ok) {
      throw new Error('فشل الاتصال بخادم الذكاء الاصطناعي');
    }

    const data = await response.json();
    const replyContent = data.choices[0].message.content;

    const finalMemory = [...newMemory, { role: 'assistant', content: replyContent }];
    saveChatMemory(finalMemory);

    return { reply: replyContent, memory: finalMemory };
  } catch (error) {
    console.error('AI Service Error:', error);
    return { 
      error: true, 
      reply: 'عذراً، حدث خطأ أثناء التواصل مع الذكاء الاصطناعي. الرجاء المحاولة لاحقاً.',
      memory: newMemory
    };
  }
}
