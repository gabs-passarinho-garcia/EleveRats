import { getWebInstrumentations, initializeFaro } from '@grafana/faro-web-sdk';
import { TracingInstrumentation } from '@grafana/faro-web-tracing';

export const initFaro = () => {
  const faroUrl = import.meta.env.VITE_FARO_URL;

  if (!faroUrl) {
    console.debug('Faro URL not defined. Skipping telemetry initialization.');
    return null;
  }

  return initializeFaro({
    url: faroUrl,
    app: {
      name: 'eleverats-frontend',
      version: '1.0.0',
      environment: import.meta.env.MODE,
    },

    instrumentations: [
      ...getWebInstrumentations({
        captureConsole: true,
      }),
      new TracingInstrumentation(),
    ],
  });
};
