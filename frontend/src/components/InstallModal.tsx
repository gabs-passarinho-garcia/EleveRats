import { useState, useEffect } from 'react';
import { usePWAInstall } from '../hooks/usePWAInstall';

export default function InstallModal(): React.JSX.Element | null {
  const { showInstallPrompt, installApp } = usePWAInstall();
  const [isVisible, setIsVisible] = useState(false);
  // Detecta se é iOS (iPhone/iPad) no render
  const userAgent = typeof window !== 'undefined' ? window.navigator.userAgent.toLowerCase() : '';
  const isIOS = /iphone|ipad|ipod/.test(userAgent);

  useEffect(() => {
    // Lógica para mostrar o modal
    const isStandalone =
      typeof window !== 'undefined'
        ? window.matchMedia('(display-mode: standalone)').matches
        : false;
    let newIsVisible = false;

    if (showInstallPrompt) {
      newIsVisible = true;
    } else if (isIOS && !isStandalone) {
      // No iOS, verificamos o sessionStorage para não ser chato demais
      const hasRefused =
        typeof sessionStorage !== 'undefined'
          ? sessionStorage.getItem('pwa_install_refused')
          : null;
      if (!hasRefused) {
        newIsVisible = true;
      }
    }

    // Set at once and typically wait for event handlers
    const timeout = setTimeout((): void => setIsVisible(newIsVisible), 0);
    return (): void => clearTimeout(timeout);
  }, [showInstallPrompt, isIOS]);

  const handleDismiss = (): void => {
    setIsVisible(false);
    sessionStorage.setItem('pwa_install_refused', 'true');
  };

  const handleInstallClick = (): void => {
    void installApp();
    setIsVisible(false);
  };

  if (!isVisible) return null;

  return (
    <div className="fixed inset-0 z-[9999] flex items-end sm:items-center justify-center bg-black/60 backdrop-blur-sm p-4 animate-fade-in-up">
      <div className="bg-white w-full max-w-sm rounded-[2rem] p-6 shadow-2xl relative overflow-hidden">
        {/* Elemento Decorativo */}
        <div className="absolute -top-10 -right-10 w-32 h-32 bg-primary-200 rounded-full blur-2xl opacity-50"></div>

        <div className="relative z-10 flex flex-col items-center text-center">
          {/* Ícone do App */}
          <div className="w-20 h-20 bg-[#0f172a] rounded-2xl shadow-lg mb-6 flex items-center justify-center border border-gray-800">
            <span className="text-3xl font-black italic tracking-tighter text-[#38bdf8]">
              EleveRats
            </span>
          </div>

          <h2 className="text-2xl font-bold text-gray-900 mb-2">Instale o EleveRats</h2>
          <p className="text-gray-500 text-sm mb-8 leading-relaxed">
            Para a melhor experiência, acesso rápido e performance visceral, adicione este app à sua
            tela inicial.
          </p>

          {isIOS ? (
            // Instruções para iOS
            <div className="w-full space-y-4 bg-gray-50 p-4 rounded-2xl border border-gray-100 mb-4">
              <div className="flex items-center gap-3 text-left">
                <span className="flex-shrink-0 w-6 h-6 bg-gray-200 rounded-full flex items-center justify-center text-xs font-bold">
                  1
                </span>
                <span className="text-sm text-gray-600">
                  Toque no botão <span className="font-bold text-blue-500">Compartilhar</span>{' '}
                  abaixo
                </span>
              </div>
              <div className="flex items-center gap-3 text-left">
                <span className="flex-shrink-0 w-6 h-6 bg-gray-200 rounded-full flex items-center justify-center text-xs font-bold">
                  2
                </span>
                <span className="text-sm text-gray-600">
                  Selecione{' '}
                  <span className="font-bold text-gray-900">Adicionar à Tela de Início</span>
                </span>
              </div>
            </div>
          ) : (
            // Botão para Android/Chrome
            <button
              onClick={handleInstallClick}
              className="w-full bg-[#38bdf8] text-[#0f172a] font-bold py-4 rounded-2xl shadow-lg active:scale-95 transition-transform mb-4 flex items-center justify-center gap-2"
            >
              <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth="2"
                  d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4"
                />
              </svg>
              Instalar Agora
            </button>
          )}

          <button
            onClick={handleDismiss}
            className="text-gray-400 font-medium text-sm py-2 hover:text-gray-600 transition-colors"
          >
            Agora não, usar no navegador
          </button>
        </div>
      </div>
    </div>
  );
}
