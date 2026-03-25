import { render, fireEvent, act } from '@testing-library/react';
import { describe, it, expect, beforeEach } from 'bun:test';
import InstallModal from './InstallModal';

describe('InstallModal', () => {
  beforeEach(() => {
    // Reset any previous state
    if (typeof sessionStorage !== 'undefined') {
      sessionStorage.clear();
    }
  });

  it('should render the modal when showInstallPrompt is true', async () => {
    const { findByText } = render(<InstallModal />);

    // Trigger the event to make showInstallPrompt true
    await act(async () => {
      window.dispatchEvent(new Event('beforeinstallprompt'));
    });

    expect(await findByText('Instale o EleveRats')).toBeTruthy();
  });

  it('should hide the modal and set sessionStorage when dismissed', async () => {
    const { findByText, queryByText } = render(<InstallModal />);

    // Trigger the event
    await act(async () => {
      window.dispatchEvent(new Event('beforeinstallprompt'));
    });

    const dismissButton = await findByText('Agora não, usar no navegador');

    await act(async () => {
      fireEvent.click(dismissButton);
    });

    expect(sessionStorage.getItem('pwa_install_refused')).toBe('true');
    expect(queryByText('Instale o EleveRats')).toBeNull();
  });

  it('should call installApp when Install Agora is clicked', async () => {
    const { findByText, queryByText } = render(<InstallModal />);

    const mockPrompt = {
      prompt: () => Promise.resolve(),
      userChoice: Promise.resolve({ outcome: 'accepted' }),
      preventDefault: () => {},
    };

    // Trigger the event with the mock
    await act(async () => {
      const event = new Event('beforeinstallprompt');
      Object.assign(event, mockPrompt);
      window.dispatchEvent(event);
    });

    const installButton = await findByText('Instalar Agora');

    await act(async () => {
      fireEvent.click(installButton);
    });

    expect(queryByText('Instale o EleveRats')).toBeNull();
  });

  it('should show iOS-specific instructions when on iOS and not standalone', async () => {
    // Mock navigator.userAgent for iOS specifically for happy-dom
    const originalUA = window.navigator.userAgent;
    // happy-dom usually allows direct assignment if not read-only, 
    // but the most reliable way is defineProperty on the prototype or instance
    try {
      Object.defineProperty(window.navigator, 'userAgent', {
        value: 'iphone',
        configurable: true,
        writable: true,
      });
    } catch {
      (window.navigator as any).userAgent = 'iphone';
    }

    // Mock matchMedia for standalone
    const originalMatchMedia = window.matchMedia;
    window.matchMedia = (query: string) => ({
      matches: false,
      media: query,
      onchange: null,
      addListener: () => {},
      removeListener: () => {},
      addEventListener: () => {},
      removeEventListener: () => {},
      dispatchEvent: () => true,
    }) as any;

    const { findByText, queryByText } = render(<InstallModal />);

    expect(await findByText(/Toque no botão/)).toBeTruthy();
    expect(queryByText('Instalar Agora')).toBeNull();

    // Cleanup
    try {
      Object.defineProperty(window.navigator, 'userAgent', {
        value: originalUA,
        configurable: true,
        writable: true,
      });
    } catch {
      (window.navigator as any).userAgent = originalUA;
    }
    window.matchMedia = originalMatchMedia;
  });
});
