import * as React from 'react'
import Api from '../../services/api'
import SearchPage from '../index'
import Loader from '../../components/loader'
import Layout from '../../components/layout'
import ShortUuidDateTime from '../../services/short-uuid-date-time'
import { DateTime } from 'luxon'

import SEO from '../../components/seo'
export const Head = () => (
    <SEO title="Reverse search" index={false} />
)

const ExistingSearchPage = ({ id }) => {
    const messages = {
        NOT_FOUND: ['Not found', 'The referenced search query does not seem to exist.'],
        UNAVAILABLE: ['Yikes!', 'The search query could not be loaded at the moment. This could indicate that Fluffle is offline. Please try again later.']
    }
    const [message, setMessage] = React.useState<string[]>();
    const [searchResult, setSearchResult] = React.useState<any>();

    React.useEffect(() => {
        Api.searchById(id, true)
            .then(data => {
                setSearchResult(data);
            })
            .catch(error => {
                const message = error.response?.status == 404 ? messages.NOT_FOUND : messages.UNAVAILABLE;
                setMessage(message);
            });
    }, []);

    const loadingPage = (
        <Layout center={true}>
            <div className="w-full flex flex-col justify-center items-center">
                <Loader />
                <div className="italic font-semibold">Retrieving search results...</div>
            </div>
        </Layout>
    );

    const messagePage = (
        <Layout center={true}>
            <div className="flex justify-center">
                {message &&
                    <div className="text-center prose max-w-lg">
                        <h1 className="m-0">{message[0]}</h1>
                        <p className="text-muted">{message[1]}</p>
                    </div>
                }
            </div>
        </Layout>
    );

    const searchPage = (
        <SearchPage searchResult={searchResult} />
    );

    return message ? messagePage : searchResult ? searchPage : loadingPage;
}

export default ExistingSearchPage
