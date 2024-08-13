import MaterialReactTable from "material-react-table";
import { useEffect, useState } from "react";
import "../../Styles/bootstrap.min.css";
import "../../Styles/bootstrap-grid.min.css";
import CssClass from "../../Styles/common.module.css";
import * as common from "../../Common/common";

/**
* Table Component

* 1. enableTopToolbar : boolean (default true)
* 2. enableColumnFilters : boolean (default false)
* 3. enableRowSelection : boolean (default false)
* 4. enableRowNumbers : boolean (default true)
* @public
*/

const Table = (props) => {
  const [tableData, setTableData] = useState([]);
  const [selectedData, setSelectedData] = useState([]);
  const [rowSelectionState, setRowSelectionState] = useState({});
  const columns = props.columns === undefined ? [] : props.columns;

  // const [tableColumns, setTableCol] = useState([]);

  const enableColumnFilters =
    props.enableColumnFilters === undefined ? false : props.enableColumnFilters;
  const enableRowSelection =
    props.enableRowSelection === undefined ? false : props.enableRowSelection;
  const enableRowNumbers =
    props.enableRowNumbers === undefined ? true : props.enableRowSelection;
  const enableTopToolbar =
    props.enableTopToolbar === undefined ? true : props.enableTopToolbar;
  const selectionDisabled =
    props.selectionDisabled === undefined ? false : props.selectionDisabled;
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
      setRowSelectionState({});
      setSelectedData([]);
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
    try {
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
    } catch (err) {
      const errorMessage = "";
    }
  }

  function checkboxChange(row, event) {
    var newState = { ...rowSelectionState, [row.id]: event.target.checked };
    var newSelectedData = [...selectedData];
    if (event.target.checked === true) {
      newSelectedData.push(row.id);
    } else {
      newSelectedData = newSelectedData.filter((d) => d !== row.id);
    }
    if (
      props.getSelectedData !== undefined &&
      typeof props.getSelectedData === "function"
    ) {
      props.getSelectedData(row.original);
    }
    setRowSelectionState(newState);
    setSelectedData(newSelectedData);
  }

  function allCheckboxChange(event) {
    if (selectionDisabled === false) {
      updateDataOnAllCheckboxTriggered(event.target.checked, tableData);
    } else {
      const availableData = tableData.filter(
        (d) => d[selectionDisabled] === "false"
      );
      var selectAll =
        availableData.length !== 0 &&
          availableData.length === selectedData.length
          ? false
          : true;
      updateDataOnAllCheckboxTriggered(selectAll, availableData);
    }
  }

  function updateDataOnAllCheckboxTriggered(checked, availableData) {
    var newState = {};
    var newSelectedData = [];
    if (checked) {
      const keys = availableData.map((d) => d.key);
      keys.forEach((k) => {
        newState = { ...newState, [k]: true };
        newSelectedData.push(k);
      });
      if (
        props.getSelectedData !== undefined &&
        typeof props.getSelectedData === "function"
      ) {
        props.getSelectedData(availableData);
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
    setSelectedData(newSelectedData);
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
        positionToolbarAlertBanner={"none"}
        muiTableTopToolbarProps={{ variant: "dense", style: { zIndex: "0" } }}
        // muiTopToolbarProps={{ style: { zIndex: '0' } }}

        enableBottomToolbar={tableData.length > 5 ? true : false}
        muiTableBottomToolbarProps={{ style: { minHeight: 40 } }}
        // muiBottomToolbarProps={{ style: { minHeight: 40} }}
        muiTableHeadCellProps={{
          style: { zIndex: "0" },
        }}
        muiTableBodyCellProps={({ cell }) => ({
          children: renderData(cell),
        })}
        enableColumnActions={false}
        enableColumnFilters={enableColumnFilters}
        getRowId={(row) => row.key}
        enableRowNumbers={enableRowNumbers}
        rowNumberMode="static"
        enableRowSelection={enableRowSelection}
        selectAllMode={"all"}
        muiSelectCheckboxProps={({ row }) => ({
          onChange: (event) => {
            checkboxChange(row, event);
          },
          disabled:
            selectionDisabled === false
              ? false
              : row.getValue(`${selectionDisabled}`) === "true",
        })}
        muiSelectAllCheckboxProps={() => ({
          onChange: (event) => {
            allCheckboxChange(event);
          },
        })}
        // muiTableContainerProps={{ sx: { maxHeight: height, maxWidth: width } }}
        muiTableContainerProps={{ sx: { maxHeight: height, maxWidth: width } }}
        muiTableBodyRowProps={props.muiTableBodyRowProps}
        initialState={{
          density: "compact",
          columnPinning: { right: ['Action'] }
        }}
        state={{
          rowSelection: rowSelectionState,
          columnVisibility: props.invisibleCol
        }}
        columns={columns}
        data={tableData}
      />
    </>
  );

  if (tableData === undefined || Object.keys(tableData).length === 0) {
    return (
      <>
        <div className={CssClass.noRows}> There is no data to be shown.</div>
      </>
    );
  } else {
    return <>{fullTableSetup}</>;
  }
};

export default Table;
