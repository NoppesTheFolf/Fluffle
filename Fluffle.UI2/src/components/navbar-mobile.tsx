import { Link } from 'gatsby'
import * as React from 'react'
import classNames from 'classnames'
import './navbar-mobile.scss'

const NavbarItemMobile = ({ href, icon, children }) => {
    return (
        <Link to={href} className="navbar-item-mobile" activeClassName="navbar-item-active-mobile">
            <span className="flex flex-col justify-center items-center">
                <div className="material-icons-outlined">{icon}</div>
                <div className="text-sm">{children}</div>
            </span>
        </Link>
    )
}

const NavbarMobile = ({ isDummy }) => {
    return (
        <nav className={classNames("flex justify-center sm:hidden pb-2 pt-4 bg-dark-300 bottom-0 left-0 w-full border-t border-dark-500 gap-x-6", { fixed: !isDummy })}>
            <NavbarItemMobile href="/" icon="youtube_searched_for">Search</NavbarItemMobile>
            <NavbarItemMobile href="/about" icon="info">About</NavbarItemMobile>
            {/* <NavbarItemMobile href="/status" icon="dns">Status</NavbarItemMobile> */}
            <NavbarItemMobile href="/api" icon="code">API</NavbarItemMobile>
            <NavbarItemMobile href="/contact" icon="mail">Contact</NavbarItemMobile>
        </nav>
    )
}

export default NavbarMobile
