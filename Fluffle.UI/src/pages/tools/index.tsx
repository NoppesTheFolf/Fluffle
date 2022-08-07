import * as React from 'react'
import Layout from '../../components/layout'
import Icon from '../../components/icon'
import { Link } from 'gatsby';

import SEO from '../../components/seo'
export const Head = () => (
    <SEO title="Tools" />
)

const Tool = ({ to, name, icon, children }) => (
    <Link to={to} className="flex items-center w-full sm:h-32 cursor-pointer select-none force-light-100 border-2 p-4 space-x-4 rounded transition-colors border-dark-100 hover:border-primary">
        <Icon name={icon} size="3rem" />
        <div>
            <div className="text-xl font-semibold">{name}</div>
            {children}
        </div>
    </Link>
);

const ToolsPage = () => {
    return (
        <Layout center={true} maxWidth="lg">
            <div className="space-y-4">
                <div className="prose text-center max-w-none">
                    <h1 className="m-0">Tools</h1>
                    <p>Apart from the website, there are a couple of tools that make using Fluffle easier.</p>
                </div>
                <div className="flex flex-col mx-auto justify-center space-y-4">
                    <Tool to="/tools/telegram-bot/" name="Telegram bot" icon="fa-telegram-plane">
                        A bot which reverse searches images sent to it. Can also be used in groups and channels.
                    </Tool>
                    <Tool to="/tools/browser-extension/" name="Browser extension" icon="web">
                        Conveniently integrate Fluffle with your desktop web browser. Available for Chrome and Firefox.
                    </Tool>
                </div>
            </div>
        </Layout>
    )
}

export default ToolsPage
