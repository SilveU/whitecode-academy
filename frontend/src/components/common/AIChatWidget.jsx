import { useState, useEffect, useRef } from 'react';
import { MessageSquare, X, Send, Trash2 } from 'lucide-react';
import { getChatMemory, sendMessageToAI, clearChatMemory } from '../../services/aiService';
import './AIChatWidget.css';

export default function AIChatWidget() {
  const [isOpen, setIsOpen] = useState(false);
  const [messages, setMessages] = useState([]);
  const [input, setInput] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const messagesEndRef = useRef(null);

  // Load initial memory (excluding system prompt)
  useEffect(() => {
    if (isOpen) {
      const memory = getChatMemory();
      const visibleMsgs = memory.filter(m => m.role !== 'system');
      setMessages(visibleMsgs);
    }
  }, [isOpen]);

  // Auto-scroll to bottom
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages, isLoading]);

  const handleSend = async (e) => {
    e.preventDefault();
    if (!input.trim() || isLoading) return;

    const userMsg = input.trim();
    setInput('');
    setMessages(prev => [...prev, { role: 'user', content: userMsg }]);
    setIsLoading(true);

    const res = await sendMessageToAI(userMsg);
    
    // Refresh visible messages from updated memory
    const visibleMsgs = res.memory.filter(m => m.role !== 'system');
    setMessages(visibleMsgs);
    setIsLoading(false);
  };

  const handleClear = () => {
    if (window.confirm('هل أنت متأكد من مسح ذاكرة المحادثة بالكامل؟')) {
      clearChatMemory();
      setMessages([]);
    }
  };

  return (
    <>
      {/* Floating Button */}
      <button 
        className="ai-chat-fab" 
        onClick={() => setIsOpen(true)}
        style={{ display: isOpen ? 'none' : 'flex' }}
      >
        <MessageSquare size={24} />
      </button>

      {/* Chat Window */}
      {isOpen && (
        <div className="ai-chat-window">
          {/* Header */}
          <div className="ai-chat-header">
            <div className="ai-chat-title">
              <MessageSquare size={18} />
              <span>White Code AI</span>
            </div>
            <div className="ai-chat-actions">
              <button onClick={handleClear} title="مسح المحادثة"><Trash2 size={16} /></button>
              <button onClick={() => setIsOpen(false)} title="إغلاق"><X size={20} /></button>
            </div>
          </div>

          {/* Messages */}
          <div className="ai-chat-messages">
            {messages.length === 0 ? (
              <div className="ai-chat-empty">
                <p>مرحباً بك! أنا مساعدك الذكي في أكاديمية وايت كود.</p>
                <p>اسألني عن الفوركس، التداول، أو أي شيء تحتاج مساعدته.</p>
              </div>
            ) : (
              messages.map((msg, idx) => (
                <div key={idx} className={`ai-message ${msg.role === 'user' ? 'user' : 'assistant'}`}>
                  <div className="bubble">
                    {msg.content.split('\n').map((line, i) => (
                      <span key={i}>
                        {line}
                        <br />
                      </span>
                    ))}
                  </div>
                </div>
              ))
            )}
            {isLoading && (
              <div className="ai-message assistant">
                <div className="bubble typing">
                  <span className="dot"></span>
                  <span className="dot"></span>
                  <span className="dot"></span>
                </div>
              </div>
            )}
            <div ref={messagesEndRef} />
          </div>

          {/* Input Area */}
          <form className="ai-chat-input" onSubmit={handleSend}>
            <input 
              type="text" 
              placeholder="اكتب سؤالك هنا..." 
              value={input}
              onChange={(e) => setInput(e.target.value)}
              disabled={isLoading}
            />
            <button type="submit" disabled={!input.trim() || isLoading}>
              <Send size={18} />
            </button>
          </form>
        </div>
      )}
    </>
  );
}
