import * as React from 'react'
import Layout from '../components/layout'
import { StaticImage } from 'gatsby-plugin-image'
import Icon from '../components/icon'

const artistName = 'Oggy123'
const artUrl = 'https://twitter.com/OggyOsbourne/status/1294700124187262976'

const socials = [
    { name: 'Telegram', url: 'https://t.me/NoppesTheFolf', icon: 'fa-telegram-plane' },
    { name: 'Twitter', url: 'https://twitter.com/NoppesTheFolf', icon: 'fa-twitter' },
    { name: 'GitHub', url: 'https://github.com/NoppesTheFolf', icon: 'fa-github-alt' }
];

const SocialBadge = ({ social }) => {
    return (
        <a className="transition-colors m-2 text-4xl fill-light-100 hover:fill-dark-400 hover:bg-light-100 p-3 rounded-full border-2" href={social.url} target="_blank" rel="noreferrer">
            <Icon inheritSize={true} name={social.icon} />
        </a>
    )
}

const ContactPage = () => {
    return (
        <Layout center={true} title="Contact">
            <div className="flex flex-col items-center space-y-6 text-center">
                <div>
                    <a href={artUrl} target="_blank" rel="noreferrer">
                        <StaticImage alt={`NoppesTheFolf icon by ${artistName}`} className="w-64 rounded-full" src="../images/noppes.jpg" />
                    </a>
                    <div className="italic">
                        <span className="text-muted">Art by </span>
                        <a href={artUrl} target="_blank" rel="noreferrer">{artistName}</a>
                    </div>
                </div>
                <div>
                    <div className="w-72 sm:w-full">
                        Found a bug? Got a feature request? Contact a folf.
                    </div>
                </div>
                <div className="flex flex-wrap justify-center items-center">
                    {socials.map(s => (
                        <SocialBadge key={s.name} social={s}></SocialBadge>
                    ))}
                </div>
            </div>
        </Layout>
    )
}

export default ContactPage
