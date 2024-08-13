import MaterialReactTable from "material-react-table";
import { useEffect, useState } from "react";
import "../../Styles/bootstrap.min.css";
import "../../Styles/bootstrap-grid.min.css";
import * as common from "../../Common/common";

/**
* Virtualized Table 
* -> This component is used for virtualized (large amount of data and without pagination)
* -> Use table component instead if data amount is few or pagination is needed
*
* 1. height : string (default 350px)
* 2. width : string (default 250px)
* 3. enableRowSelection : boolean (default true)
* 4. enableSelectAll : boolean (default true)
* 5. enableTopToolbar : boolean (default false)
* 5. rowSelectionState : object (default cleared upon data changed)
* 6. selectAsRadio : boolean -> use checkbox as radio button, enableSelectAll must be set to false (default false)

* @public
*/

const VirtualTable = (props) => {
  const [tableData, setTableData] = useState([]);
  const [rowSelectionState, setRowSelectionState] = useState({});
  const columns = props.columns === undefined ? [] : props.columns;

  const enableRowSelection =
    props.enableRowSelection === undefined ? true : props.enableRowSelection;
  const enableSelectAll =
    props.enableSelectAll === undefined ? true : props.enableSelectAll;
  const enableTopToolbar =
    props.enableTopToolbar === undefined ? false : props.enableTopToolbar;
  const enableStickyHeader =
    props.enableStickyHeader === undefined ? false : props.enableStickyHeader;
  // const height =
  //   props.height === undefined && props.maxHeight === undefined
  //     ? ""
  //     : props.maxHeight || props.height;
  // const width =
  //   props.width === undefined && props.maxWidth === undefined
  //     ? "100%"
  //     : props.width || props.maxWidth;

  const height =
    props.height === undefined && props.maxHeight === undefined
      ? "570px"
      : props.maxHeight || props.height;
  const width =
    props.width === undefined && props.maxWidth === undefined
      ? "92vw"
      : props.width || props.maxWidth;

  useEffect(() => {
    if (props.data !== undefined) {
      setTableData(props.data);
      setRowSelectionState(
        props.rowSelectionState === undefined ? {} : props.rowSelectionState
      );
    }
  }, [props.data]);

  // const [tableHeader, setTableHeader] = useState("");

  // useEffect(() => {
  //   if (props.columns !== undefined) {
  //     // console.log("There are columns for table header.");
  //     for (var i = 0; i < props.columns.length; i++) {
  //       let temp = "";
  //       if (props.columns[i].header.length < 25) {
  //         for (var j = 0; j < 27 - props.columns[i].header.length; j++) {
  //           temp += " ";
  //         }
  //         props.columns[i].header += temp;
  //         // console.log(props.columns[i].header);
  //       }
  //       setTableHeader(props.columns);
  //     }
  //   }
  // }, [props.columns]);

  function renderData(cell) {
    if (
      cell.getValue() === undefined ||
      cell.getValue() === null ||
      cell.getValue() === "" ||
      cell.getValue() === "0001-01-01T00:00:00"
    ) {
      cell.renderValue = () => (
        <>
          <label> - </label>
        </>
      );
    } else {
      const val = common.c_Dis_DateTime(cell.getValue()).dis;
      if (val !== "-" && typeof val !== "object") {
        cell.renderValue = () => (
          <>
            <label> {val} </label>
          </>
        );
      }
    }
  }

  function radioFn(row) {
    var newState = { [row.id]: true };
    setRowSelectionState(newState);
    if (
      props.getSelectedData !== undefined &&
      typeof props.getSelectedData === "function"
    ) {
      props.getSelectedData(row.original);
    }
  }

  function checkboxChange(row, event) {
    var newState = { ...rowSelectionState, [row.id]: event.target.checked };
    setRowSelectionState(newState);
    if (
      props.getSelectedData !== undefined &&
      typeof props.getSelectedData === "function"
    ) {
      props.getSelectedData(row.original);
    }
  }

  function allCheckboxChange(event) {
    var newState = {};
    if (event.target.checked === true) {
      const keys = tableData.map((d) => d.key);
      keys.forEach((k) => {
        newState = { ...newState, [k]: true };
      });
      if (
        props.getSelectedData !== undefined &&
        typeof props.getSelectedData === "function"
      ) {
        props.getSelectedData(tableData);
      }
    } else {
      if (
        props.getSelectedData !== undefined &&
        typeof props.getSelectedData === "function"
      ) {
        props.getSelectedData({});
      }
    }
    setRowSelectionState(newState);
  }

  const fullTableSetup = (
    <>
      <MaterialReactTable
        enableTopToolbar={enableTopToolbar}
        enableStickyHeader={enableStickyHeader}
        // enableStickyHeader={true}
        enableFullScreenToggle={false}
        enableDensityToggle={false}
        enableHiding={false}
        enableColumnActions={false}
        enableColumnFilters={false}
        positionToolbarAlertBanner={"none"}
        muiTableTopToolbarProps={{ variant: "dense", style: { zIndex: "0" } }}
        // muiTopToolbarProps={{ variant: 'dense' }}

        enableBottomToolbar={false}
        enablePagination={false}
        getRowId={(row) => row.key}
        enableRowSelection={enableRowSelection}
        enableSelectAll={enableSelectAll}
        selectAllMode={"all"}
        muiSelectCheckboxProps={({ row }) => ({
          onChange: (event) => {
            if (enableSelectAll !== true && props.selectAsRadio === true) {
              radioFn(row);
            } else {
              checkboxChange(row, event);
            }
          },
          color: "primary",
        })}
        muiSelectAllCheckboxProps={() => ({
          onChange: (event) => {
            allCheckboxChange(event);
          },
        })}
        muiTableHeadCellProps={{
          style: { zIndex: "0" },
        }}
        muiTableContainerProps={{
          sx: { maxHeight: height, maxWidth: width },
        }}
        muiTableBodyCellProps={({ cell }) => ({
          children: renderData(cell),
          style: { zIndex: "0" },
        })}
        initialState={{ density: "compact" }}
        state={{ rowSelection: rowSelectionState }}
        columns={columns}
        data={tableData}
      />
    </>
  );

  return <>{fullTableSetup}</>;
};

export default VirtualTable;
