import CssClass from "../../Styles/common.module.css";
import { Button } from "../../Common/CommonComponents";

const SearchPanel = (props) => {
  const onFormSubmit = (e) => {
    e.preventDefault();
    props.onSubmit();
    // send state to server with e.g. `window.fetch`
  };

  return (
    <form onSubmit={onFormSubmit} style={{ padding: "0 1rem 0 1rem" }}>
      <ul className={CssClass.breadcrumb3}>
        <li className={CssClass.breadcrumb2Item}>Search Panel</li>
      </ul>

      <div className={CssClass.flexContainer2}>
        <div className={CssClass.flexItemLeft}>
          <table className="table table-bordered table-hover">
            <tbody>{props.children}</tbody>
          </table>
        </div>
      </div>
      <div style={{ display: "none" }}>
        <Button id="submitBtn" type="submit">
          {" "}
          SUBMIT{" "}
        </Button>
      </div>
    </form>
  );
};

export default SearchPanel;
