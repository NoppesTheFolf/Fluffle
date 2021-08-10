import * as React from 'react'
import Layout from '../components/layout'
import { Helmet } from 'react-helmet'

const IndexPage = () => {
    return (
        <Layout center={true} title="Reverse search">
            <Helmet>
                <meta name="description" content="A reverse image search service tailored to the furry community."></meta>
            </Helmet>
            Reverse search
        </Layout>
    )
}

export default IndexPage
