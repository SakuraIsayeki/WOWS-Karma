import '../styles/app.scss'
import 'bootswatch/dist/darkly/bootstrap.min.css';
import type { AppProps } from 'next/app'
import Layout from "../components/layout";

function App({ Component, pageProps }: AppProps) {
  return(
      <Layout>
        <Component {...pageProps} />
      </Layout>
  )
}

export default App
