import * as React from 'react'
import { Link } from "gatsby"
import Icon from "../../components/icon"

const BackToTools = () => (
    <Link to="/tools/" className="flex items-center space-x-1">
        <Icon name="arrow-back-ios" size="1rem" />
        <span>View all tools</span>
    </Link>
)

export default BackToTools
