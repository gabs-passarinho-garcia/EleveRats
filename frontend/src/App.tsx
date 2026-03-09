import React from 'react';
import InstallModal from './components/InstallModal';

function App(): React.JSX.Element {
  return (
    <div className="min-h-screen bg-primary flex flex-col items-center justify-center p-4 relative overflow-hidden font-sans">
      <InstallModal />

      {/* Background decoration */}
      <div className="absolute top-0 left-0 w-full h-full opacity-10 pointer-events-none">
        <div className="absolute top-10 left-10 w-32 h-32 bg-secondary rounded-full blur-3xl"></div>
        <div className="absolute bottom-10 right-10 w-48 h-48 bg-secondary rounded-full blur-3xl"></div>
      </div>

      <main className="z-10 flex flex-col items-center max-w-2xl w-full text-center">
        <div className="mb-8 relative">
          <img
            src="/jesus_maromba.png"
            alt="Jesus Maromba"
            className="w-48 h-48 md:w-64 md:h-64 object-cover rounded-3xl border-4 border-secondary shadow-[0_0_30px_rgba(56,189,248,0.3)] filter grayscale hover:grayscale-0 transition-all duration-500"
          />
        </div>

        <h1 className="text-5xl md:text-7xl font-black text-secondary uppercase tracking-tighter mb-4 drop-shadow-lg">
          Eleve<span className="text-white">Rats</span>
        </h1>

        <p className="text-xl md:text-2xl text-gray-400 font-medium mb-12 tracking-wide uppercase max-w-lg">
          Sistema de gestão e academia brutalista.
        </p>

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 w-full">
          <button className="bg-secondary text-primary font-bold uppercase tracking-wider py-4 px-8 rounded-xl hover:bg-white hover:text-primary transition-colors border-2 border-secondary shadow-[0_0_15px_rgba(56,189,248,0.4)]">
            Acessar Cidadela
          </button>
          <button className="bg-transparent text-secondary font-bold uppercase tracking-wider py-4 px-8 rounded-xl hover:bg-secondary/10 transition-colors border-2 border-secondary border-dashed">
            Forjar Registro
          </button>
        </div>
      </main>

      <footer className="absolute bottom-4 text-gray-600 text-sm font-bold uppercase tracking-widest">
        EST. 2024 • THE GAINS ARE ETERNAL
      </footer>
    </div>
  );
}

export default App;
