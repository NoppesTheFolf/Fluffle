import * as React from 'react'
import SearchPage from './index'

import SEO from '../components/seo'
export const Head = () => (
    <SEO title="Reverse search" />
)

const BrowserExtension = () => {
    return (
        <SearchPage forBrowserExtension={true} />
    )
}

export default BrowserExtension
