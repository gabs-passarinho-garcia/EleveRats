import { render, fireEvent, act } from '@testing-library/react';
import { describe, it, expect, mock, beforeEach } from 'bun:test';
import InstallModal from './InstallModal';

mock.module('../hooks/usePWAInstall', () => ({
  usePWAInstall: mock(() => ({
    showInstallPrompt: true,
    installApp: mock(),
    canInstall: true,
  })),
}));

describe('InstallModal', () => {
  beforeEach(() => {
    // Reset any previous state
    if (typeof sessionStorage !== 'undefined') {
      sessionStorage.clear();
    }
  });

  it('should render the modal when showInstallPrompt is true', async () => {
    const rendered = render(<InstallModal />);

    // allow effects to run
    await act(async () => {
      await new Promise((r) => setTimeout(r, 0));
    });

    expect(rendered.getByText('Instale o EleveRats')).toBeTruthy();
  });

  it('should hide the modal and set sessionStorage when dismissed', async () => {
    const rendered = render(<InstallModal />);

    // allow effects to run
    await act(async () => {
      await new Promise((r) => setTimeout(r, 0));
    });

    const dismissButton = rendered.getByText('Agora não, usar no navegador');

    await act(async () => {
      fireEvent.click(dismissButton);
    });

    expect(sessionStorage.getItem('pwa_install_refused')).toBe('true');
  });
});
