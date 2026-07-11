import type { Metadata } from 'next';
import { Inter } from 'next/font/google';
import { Provider } from '@/components/provider';
import './global.css';

const inter = Inter({
  subsets: ['latin'],
});

export const metadata: Metadata = {
  metadataBase: new URL('https://aerafaldev.github.io/HostFxrSharp'),
  title: {
    default: 'HostFxrSharp',
    template: '%s · HostFxrSharp',
  },
  description:
    'A safe, Native-AOT-compatible managed wrapper over the native .NET hosting components (hostfxr and nethost).',
};

export default function Layout({ children }: LayoutProps<'/'>) {
  return (
    <html lang="en" className={inter.className} suppressHydrationWarning>
      <body className="flex flex-col min-h-screen">
        <Provider>{children}</Provider>
      </body>
    </html>
  );
}
