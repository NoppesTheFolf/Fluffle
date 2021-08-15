import classNames from 'classnames'
import * as React from 'react'
import Icon from './icon'

const Accordion = ({ children, initiallyOpen, header, fontSize }) => {
    const [isOpen, setIsOpen] = React.useState(initiallyOpen);

    return (
        <div>
            <div className="w-min flex items-center cursor-pointer select-none" onClick={() => setIsOpen(!isOpen)}>
                <div className="mr-1" style={{ fontSize: fontSize + "rem" }}>
                    <Icon name={`expand-${isOpen ? "less" : "more"}`} />
                </div>
                <div dangerouslySetInnerHTML={{ __html: header }}>
                </div>
            </div>
            <div className={classNames({ hidden: !isOpen })}>
                {children}
            </div>
        </div>
    );
};

Accordion.defaultProps = {
    initiallyOpen: false
}

export default Accordion
