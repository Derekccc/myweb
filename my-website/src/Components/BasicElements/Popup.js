import React from 'react'

function Popup(props) {
    return (props.trigger) ? (
        <div className="popup">
            <div className="popup-inner">
                <h3><b><span>Warning</span></b></h3>
                <div><b>{props.children}</b></div>
                <div className="divbtnOk">
                    <button className="btn btnPrimary" onClick={()=>props.setTrigger(false)}>
                        <div className="checkmark"></div>
                        <span>OK</span></button>
                </div>
            </div>
        </div>
    ) : "";
}
export default Popup