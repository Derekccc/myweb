import * as Comp from "../../../Common/CommonComponents";
import { useState, useEffect } from "react";

const RAConsumerInfoTable = (props) => {
  const [tableData, setTableData] = useState([]);
  const [writeAuthority, setWriteAuthority] = useState(false);

  useEffect(() => {
    if (props.data !== undefined) {
      setTableData(props.data);
    }
    if (props.actionAuthority !== undefined) {
      setWriteAuthority(props.actionAuthority.WRITE === "Y");
    }
  }, [props.data, props.actionAuthority]);

  function checkBoxHandler(row, val, featureType) {
    const newData = [...tableData];
    const no = newData[row.rowNo].AUTHORITY_DETAILS.filter((d) => d.FEATURE_TYPE === featureType).map((f) => f.rowNo)[0];
    try {
      if (featureType !== "R" && val === true) {
        const detailR = newData[row.rowNo].AUTHORITY_DETAILS.filter((d) => d.FEATURE_TYPE === "R");
        if (detailR.length > 0 && newData[row.rowNo].AUTHORITY_DETAILS[detailR[0].rowNo].AUTHORIZED === "N") {
          var featureIdR = row.AUTHORITY_DETAILS[detailR[0].rowNo].FEATURE_ID;
          props.ChangeFeatureAuthority(featureIdR, val);
          newData[row.rowNo].AUTHORITY_DETAILS[detailR[0].rowNo].AUTHORIZED = 
            val === true ? "Y" : "N";
        }
      }
      var featureId = row.AUTHORITY_DETAILS[no].FEATURE_ID;
      props.ChangeFeatureAuthority(featureId, val);
      newData[row.rowNo].AUTHORITY_DETAILS[no].AUTHORIZED = val === true ? "Y" : "N";
    } finally {
      setTableData([...newData]);
    }
  }

  const columns = [
    {
      accessorKey: "MODULE_NAME",
      header: "Module",
      size: 200,
    },
    {
      id: "FEATURE_TYPE_1",
      header: "Read",
      size: 60,
      accessorFn: (row) => {
        const detailR = row.AUTHORITY_DETAILS.filter((d) => d.FEATURE_TYPE === "R");
        const detailW = row.AUTHORITY_DETAILS.filter((d) => d.FEATURE_TYPE === "W");
        const existFlagR = row.AUTHORITY_DETAILS.length > 0 ? detailR.length > 0 : false;
        const existFlagW = row.AUTHORITY_DETAILS.length > 0 ? detailW.length > 0 : false;
        
        console.log("Row:", row);
        console.log("existFlagR:", existFlagR);
        console.log("existFlagW:", existFlagW);
        console.log("writeAuthority:", writeAuthority);

        return (
          <>
            <Comp.Input
              id={`${row.MODULE_NAME}_FEATURE_TYPE_1`}
              name="READ"
              type="checkbox"
              className=""
              disabled={!existFlagR || (existFlagW ? detailW[0].AUTHORIZED === "Y" : false) || !writeAuthority}
              checked={existFlagR ? detailR[0].AUTHORIZED === "Y" : false}
              onChange={(e) => checkBoxHandler(row, e.target.checked, "R")}
            />
          </>
        );
      },
    },
    {
      id: "FEATURE_TYPE_2",
      header: "Write",
      size: 60,
      accessorFn: (row) => {
        const detail = row.AUTHORITY_DETAILS.filter((d) => d.FEATURE_TYPE === "W");
        const existFlag = row.AUTHORITY_DETAILS.length > 0 ? detail.length > 0 : false;
        return (
          <>
            <Comp.Input
              id={`${row.MODULE_NAME}_FEATURE_TYPE_2`}
              name="WRITE"
              type="checkbox"
              className=""
              disabled={!existFlag || !writeAuthority}
              checked={existFlag ? detail[0].AUTHORIZED === "Y" : false}
              onChange={(e) => checkBoxHandler(row, e.target.checked, "W")}
            />
          </>
        );
      },
    },
  ];

  return (
    <>
      <Comp.VirtualTable
        columns={columns}
        data={tableData}
        enableRowSelection={false}
        height={"1000px"}
        width={"100vw"}
      />
    </>
  );
};

export default RAConsumerInfoTable;
