import { Link } from 'gatsby'
import * as React from 'react'
import { navbarItem, navbarItemActive } from './navbar-mobile.module.scss'
import Icon from './icon'

const NavbarItemMobile = ({ href, icon, children }) => {
    return (
        <Link to={href} target={href.startsWith("http") ? '_blank' : '_self'} className={navbarItem} partiallyActive={href !== '/'} activeClassName={navbarItemActive}>
            <span className="flex flex-col justify-center items-center">
                <Icon name={icon} />
                <div className="text-sm">{children}</div>
            </span>
        </Link>
    )
}

const NavbarMobile = React.forwardRef((_, ref) => {
    return (
        <nav ref={ref} className="fixed bottom-0 left-0 flex justify-around sm:hidden pb-2 pt-4 bg-dark-300 w-full border-t border-dark-500 z-50">
            <NavbarItemMobile href="/" icon="youtube-searched-for">Search</NavbarItemMobile>
            <NavbarItemMobile href="/tools/" icon="handyman">Tools</NavbarItemMobile>
            <NavbarItemMobile href="/about/" icon="info">About</NavbarItemMobile>
            <NavbarItemMobile href="https://fluffle.readme.io/reference/search-api-introduction#/" icon="code">API</NavbarItemMobile>
            <NavbarItemMobile href="/contact/" icon="mail">Contact</NavbarItemMobile>
            <NavbarItemMobile href="https://ko-fi.com/noppesthefolf" icon="ko-fi">Ko-fi</NavbarItemMobile>
        </nav>
    )
})

export default NavbarMobile
