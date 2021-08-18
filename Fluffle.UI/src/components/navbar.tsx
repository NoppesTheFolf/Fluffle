import { Link } from 'gatsby'
import * as React from 'react'
import Svg from '../services/svg'
import Icon from './icon'
import { navbarItem, navbarItemActive } from './navbar.module.scss'

const NavbarItem = ({ href, icon, children }) => {
    return (
        <Link to={href} activeClassName={navbarItemActive} className={navbarItem}>
            <span className="hidden md:inline"><Icon name={icon} /></span>
            <span>{children}</span>
        </Link>
    )
}

const Navbar = () => {
    return (
        <header className="hidden sm:flex justify-between items-center py-2 w-full">
            <Link to="/" className="flex items-center text-light-100 hover:text-light-200 cursor-pointer select-none">
                <img className="h-8 mr-2" src={Svg.get("tail")} alt="Fluffle brand logo"></img>
                <span className="block text-xl">Fluffle</span>
            </Link>
            <nav>
                <div className="flex flex-wrap space-x-2 md:space-x-3 lg:space-x-8">
                    <NavbarItem href="/" icon="youtube-searched-for">Reverse search</NavbarItem>
                    <NavbarItem href="/about/" icon="info">About</NavbarItem>
                    {/* <NavbarItem href="/status/" icon="dns">Status</NavbarItem> */}
                    <NavbarItem href="/api/" icon="code">API</NavbarItem>
                    <NavbarItem href="/contact/" icon="mail">Contact</NavbarItem>
                </div>
            </nav>
        </header>
    )
}

export default Navbar
