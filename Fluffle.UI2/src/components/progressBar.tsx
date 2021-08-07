import * as React from 'react'

const ProgressBar = ({ children }) => {
    return (
        <div className="w-full">
            <div className="relative w-full h-4 overflow-hidden rounded bg-gradient-secondary">
                {children}
            </div>
        </div>
    )
}

export default ProgressBar
