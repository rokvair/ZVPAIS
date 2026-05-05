import React, { createContext, useContext, useState } from 'react';
import { T } from '../translations';

const LanguageContext = createContext();

export function LanguageProvider({ children }) {
  const [lang, setLang] = useState(() => localStorage.getItem('lang') || 'lt');
  const toggle = () => setLang(l => {
    const next = l === 'lt' ? 'en' : 'lt';
    localStorage.setItem('lang', next);
    return next;
  });
  const t = (key) => T[key]?.[lang] ?? T[key]?.lt ?? key;
  return (
    <LanguageContext.Provider value={{ lang, toggle, t }}>
      {children}
    </LanguageContext.Provider>
  );
}

export function useLanguage() {
  return useContext(LanguageContext);
}