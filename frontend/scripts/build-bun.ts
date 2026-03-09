#!/usr/bin/env bun
/**
 * Build script using Bun's native bundler
 * Processes React app and copies necessary assets
 */

import { existsSync, rmSync } from 'fs';
import { cp, mkdir, readFile, writeFile } from 'fs/promises';
import { join } from 'path';

const distDir = './dist';
const publicDir = './public';

console.log('🚀 Building with Bun bundler...\n');

// Clean dist directory
if (existsSync(distDir)) {
  rmSync(distDir, { recursive: true, force: true });
}
await mkdir(distDir, { recursive: true });

// Build with Bun's native bundler
console.log('📦 Bundling JavaScript with Bun...');
const result = await Bun.build({
  entrypoints: ['./src/main.tsx'],
  outdir: './dist',
  target: 'browser',
  minify: true,
  sourcemap: 'external',
  splitting: true,
  publicPath: './',
  external: [],
});

if (!result.success) {
  console.error('❌ Build failed:', result.logs);
  process.exit(1);
}

console.log('✅ JavaScript bundled successfully');

// Copy public assets
if (existsSync(publicDir)) {
  console.log('📋 Copying public assets...');
  try {
    await cp(publicDir, distDir, { recursive: true });
  } catch (error) {
    console.warn('⚠️  Could not copy public assets:', error);
  }
}

// Copy and process index.html
if (existsSync('./index.html')) {
  console.log('📄 Processing index.html...');
  let html = await readFile('./index.html', 'utf-8');

  // Update script src to point to bundled file
  const mainBundle = result.outputs.find(
    (output) => output.path.includes('main') || output.path.includes('index'),
  );

  if (mainBundle) {
    const bundleName = mainBundle.path.split('/').pop();
    html = html.replace(
      /<script type="module" src="\/src\/main\.tsx"><\/script>/,
      `<script type="module" src="./${bundleName}"></script>`,
    );
  }

  await writeFile(join(distDir, 'index.html'), html);
}

// Process CSS with PostCSS/Tailwind
console.log('🎨 Processing CSS with Tailwind...');
try {
  const { execSync } = await import('child_process');
  execSync('bun x --bun postcss ./src/index.css -o ./dist/index.css --minify', {
    stdio: 'inherit',
  });
} catch (error) {
  console.warn('⚠️  Could not process CSS, copying raw file:', error);
  if (existsSync('./src/index.css')) {
    await cp('./src/index.css', './dist/index.css');
  }
}

console.log('\n✅ Build complete! Output in ./dist');
console.log('📝 Note: For full PWA support with service workers, use "bun run build:vite"');
