import { renderHook, act } from '@testing-library/react';
import { describe, it, expect, mock } from 'bun:test';
import { usePWAInstall } from './usePWAInstall';

describe('usePWAInstall', () => {
  it('should initialize with default state', () => {
    const { result } = renderHook(() => usePWAInstall());
    expect(result.current.showInstallPrompt).toBe(false);
    expect(result.current.canInstall).toBe(false);
  });

  it('should update state when beforeinstallprompt event is fired', () => {
    const { result } = renderHook(() => usePWAInstall());

    // Use a custom event or a plain event with required properties
    const event = new Event('beforeinstallprompt', { cancelable: true });
    const preventDefaultSpy = mock(() => {});
    event.preventDefault = preventDefaultSpy;

    act(() => {
      window.dispatchEvent(event);
    });

    expect(result.current.showInstallPrompt).toBe(true);
    expect(result.current.canInstall).toBe(true);
    expect(preventDefaultSpy).toHaveBeenCalled();
  });

  it('should handle installApp when accepted', async () => {
    const { result } = renderHook(() => usePWAInstall());

    const promptSpy = mock(() => Promise.resolve());
    const event = new Event('beforeinstallprompt', { cancelable: true });
    Object.assign(event, {
      prompt: promptSpy,
      userChoice: Promise.resolve({ outcome: 'accepted' }),
    });

    act(() => {
      window.dispatchEvent(event);
    });

    await act(async () => {
      await result.current.installApp();
    });

    expect(promptSpy).toHaveBeenCalled();
    expect(result.current.showInstallPrompt).toBe(false);
    expect(result.current.canInstall).toBe(false);
  });

  it('should handle installApp when dismissed', async () => {
    const { result } = renderHook(() => usePWAInstall());

    const promptSpy = mock(() => Promise.resolve());
    const event = new Event('beforeinstallprompt', { cancelable: true });
    Object.assign(event, {
      prompt: promptSpy,
      userChoice: Promise.resolve({ outcome: 'dismissed' }),
    });

    act(() => {
      window.dispatchEvent(event);
    });

    await act(async () => {
      await result.current.installApp();
    });

    expect(promptSpy).toHaveBeenCalled();
    expect(result.current.showInstallPrompt).toBe(false);
  });

  it('should not do anything if installApp is called without deferredPrompt', async () => {
    const { result } = renderHook(() => usePWAInstall());

    await act(async () => {
      await result.current.installApp();
    });

    expect(result.current.showInstallPrompt).toBe(false);
  });
});
