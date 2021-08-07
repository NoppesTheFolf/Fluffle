import * as React from 'react'
import Layout from '../components/layout'

const NotFoundPage = () => {
    return (
        <Layout center={true} title="Not found">
            <div className="text-7xl font-light mb-4">Blimey! 404 Not Found</div>
            <div className="text-muted text-3xl font-light">We couldn't find the page you're looking for</div>
        </Layout>
    )
}

export default NotFoundPage
