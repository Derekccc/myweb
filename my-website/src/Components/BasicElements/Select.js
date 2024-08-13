import React from "react";
import PropTypes from "prop-types";
import CssClass from "../../Styles/common.module.css";
import { default as ReactSelect } from "react-select";

const Select = (props) => {
  const errorMessage = props.errorMessage === undefined ? "" : props.errorMessage;

  // if (props.readOnly) {
  //   return (
  //     <div
  //       className={`form-group ${props.label !== undefined && CssClass.modalGroup
  //         }`}
  //     >
  //       {props.label !== undefined && (
  //         <label className={CssClass.colFormLabel} htmlFor={props.id}>
  //           {props.label}
  //         </label>
  //       )}
  //       <div
  //         className={`${props.className !== undefined
  //             ? CssClass[props.className]
  //             : CssClass.modalPopupItem
  //           } ${props.isValid === false && CssClass.invalidField}`}
  //       >
  //         <ReactSelect {...props}
  //           isClearable={!props.readOnly}
  //           isSearchable={!props.readOnly}
  //           openMenuOnClick={!props.readOnly} />
  //       </div>
  //     </div>
  //   );
  // }

  if (props.allowSelectAll) {
    return (
      <div
        className={`form-group ${props.label !== undefined && CssClass.modalGroup
          }`}
      >
        {props.label !== undefined && (
          <label className={CssClass.colFormLabel} htmlFor={props.id}>
            {props.label}
          </label>
        )}
        <div
          className={`${CssClass.modalPopupItem} ${props.isValid === false && CssClass.invalidField
            }`}
        >
          <ReactSelect
            {...props}
            menuPortalTarget={document.querySelector("body")}
            options={
              props.options !== undefined
                ? [props.allOption, ...props.options]
                : []
              
            }
            onChange={(selected, event) => {
              if (selected !== null && selected.length > 0) {
                if (
                  selected[selected.length - 1].value === props.allOption.value
                ) {
                  return props.onChange([props.allOption], event);
                }
                let result = [];

                if (selected.includes(props.allOption)) {
                  if (selected.length === props.options.length + 1) {
                    return props.onChange([props.allOption]);
                  } else {
                    result = selected.filter(
                      (option) => option.value !== props.allOption.value,
                    );
                    return props.onChange(result, event);
                  }
                } else {
                  if (selected.length === props.options.length) {
                    return props.onChange([props.allOption], event);
                  } else {
                    return props.onChange(selected, event);
                  }
                }
              }

              return props.onChange(selected, event);
            }}
            
          />
        </div>
        {errorMessage !== "" && (
          <div className={CssClass.invalidMessage}>{props.errorMessage}</div>
        )}
      </div>
    );
  }

  return (
    <div
      className={`form-group ${props.label !== undefined && CssClass.modalGroup
        }`}
    >
      {props.label !== undefined && (
        <label className={CssClass.colFormLabel} htmlFor={props.id}>
          {props.label}
        </label>
      )}
      <div
        className={`${props.className !== undefined
            ? CssClass[props.className]
            : CssClass.modalPopupItem
          } ${props.isValid === false && CssClass.invalidField}`}
      >
        <ReactSelect {...props} />
      </div>
      {errorMessage !== "" && (
        <div className={CssClass.invalidMessage}>{props.errorMessage}</div>
      )}
    </div>
  );
};

Select.propTypes = {
  options: PropTypes.array,
  value: PropTypes.any,
  onChange: PropTypes.func,
  readOnly: PropTypes.bool,
  allowSelectAll: PropTypes.bool,
  allOption: PropTypes.shape({
    label: PropTypes.string,
    value: PropTypes.string,
  }),
};

Select.defaultProps = {
  allOption: {
    label: "Select all",
    value: "-1",
  },
};

export default Select;
