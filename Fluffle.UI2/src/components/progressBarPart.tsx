import * as React from 'react'

const ProgressBarPart = ({ color, percentage }) => {
    return (
        <div className={`absolute top-0 left-0 bg-gradient-${color} h-full transition-all`} style={{ width: `${percentage}%` }}></div>
    )
}

export default ProgressBarPart
