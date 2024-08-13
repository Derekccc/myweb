import React, { useState, useEffect } from "react";
import { useCookies } from "react-cookie";
import { useNavigate } from "react-router-dom";
import { toast } from "react-toastify";
import { GetPageAuthorityFn } from "../../Common/authority";
import http from "../../Common/http-common";
import * as common from "../../Common/common";
import * as Comp from "../../Common/CommonComponents";
import CssClass from "../../Styles/common.module.css";
import { Bar } from "react-chartjs-2";
import { Chart as ChartJS, CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend } from "chart.js";
import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";

ChartJS.register(CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend);

const PAGE_NAME = "SalesChartHomePage.js_";
const MODULE_ID = "WEB_CS_004";

const SalesChartHomePage = () => {
  const navigate = useNavigate();
  const [cookies] = useCookies([]);
  const [userId] = useState(cookies.USER_ID);
  const [chartData, setChartData] = useState({
    labels: [],
    datasets: [
      {
        label: 'Total Amount',
        data: [],
        backgroundColor: 'rgba(75, 192, 192, 0.6)',
      },
    ],
  });
  const [actionAuthority, setActionAuthority] = useState({
    READ: "N",
    WRITE: "N",
  });

  
  const [startDate, setStartDate] = useState(null);
  const [endDate, setEndDate] = useState(null);
  const [loading, setLoading] = useState(false);
  const [loadingText, setLoadingText] = useState("Loading...");

  console.log("start", startDate);
  console.log("end", endDate);


  useEffect(() => {
    const fetchAuthority = async () => {
      try {
        const result = await GetPageAuthorityFn(userId, MODULE_ID);
        setActionAuthority(result);
        if (result.READ === "N") {
          navigate("/");
        }
      } catch (error) {
        setActionAuthority({ READ: "N", WRITE: "N" });
        common.c_LogWebError(PAGE_NAME, "GetPageAuthorityFn", error);
        toast.error(error);
        navigate("/");
      }
    };

    fetchAuthority();
  }, [userId, navigate]);

  // useEffect(() => {
  //   if (actionAuthority.READ === "Y") {
  //     fetchSalesChartData();
  //   }
  // }, [startDate, endDate, actionAuthority, userId]);

  const fetchSalesChartData = () => {
    if (!startDate || !endDate) {
      toast.error('Please select both start date and end date.');
      return;
    }
  
    setLoading(true);
  
    // const endDateAdjusted = new Date(endDate.getTime());
    // endDateAdjusted.setHours(23, 59, 59, 999); 
  
    const queryParams = {
      updateId: userId,
      startDate: startDate,
      endDate: endDate,
    };
  
    http.get(`api/salesOrder/GetSalesChartData`, { params: queryParams })
      .then(response => {
        console.log('API response:', response.data);
  
        if (response.data && Array.isArray(response.data)) {
          const data = response.data;
          const chartLabels = data.map(item => `${item.OrderMonth.toString().padStart(2, '0')}/${item.OrderYear}`); // Format MM/YYYY
          const chartValues = data.map(item => item.TotalAmount);
  
          setChartData({
            labels: chartLabels,
            datasets: [
              {
                label: 'Total Amount',
                data: chartValues,
                backgroundColor: 'rgba(75, 192, 192, 0.6)',
              },
            ],
          });
        } else {
          console.error('Invalid data format received:', response.data);
          toast.error('Invalid data format received from server.');
        }
      })
      .catch(error => {
        console.error('Error fetching sales chart data:', error);
        toast.error('Error fetching sales chart data. Please try again.');
      })
      .finally(() => {
        setLoading(false);
      });
  };  
  

  const clearDates = () => {
    setStartDate(null);
    setEndDate(null);
    setChartData({
      labels: [],
      datasets: [
        {
          label: 'Total Amount',
          data: [],
          backgroundColor: 'rgba(75, 192, 192, 0.6)',
        },
      ],
    });
  };

  return (
    <>
      <Comp.Header>Sales Chart Date Range</Comp.Header>
      <div className="date-selection-container" style={{ display: "flex", justifyContent: "center", gap: "20px", marginBottom: "20px" }}>
        <div className="date-input-group" style={{ display: "flex", alignItems: "center", gap: "20px" }}>
          <label>Start Date:</label>
          <DatePicker
            selected={startDate}
            onChange={date => setStartDate(date)}
            dateFormat="dd/MM/yyyy"
            className="date-picker-input"
            placeholderText="Select start date"
          />
          &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
          <label>End Date:</label>
          <DatePicker
            selected={endDate}
            onChange={date => setEndDate(date)}
            dateFormat="dd/MM/yyyy"
            className="date-picker-input"
            placeholderText="Select end date"
            minDate={startDate} // Ensure end date cannot be before start date
          />

        </div>
      </div>
      <div style={{ display: "flex", justifyContent: "flex-end", gap: "10px", marginBottom: "20px" }}>
      {actionAuthority.WRITE === "Y" && (
        <>
        <Comp.Button id="btnSearch" type="general" onClick={fetchSalesChartData}>
          SEARCH
        </Comp.Button>
        <Comp.Button id="btnClear" type="general" onClick={clearDates}>
          CLEAR
        </Comp.Button>
        </>
        )}
      </div>
      
      <div style={{ backgroundColor: '#A52A2A', padding: "20px", borderRadius: "10px" }}>
        <div className={`${CssClass.cardTwo} ${CssClass.tableCardTwo}`}>
          <h3 className={CssClass.tableTitle}>Sales Chart Analyst</h3>
          <hr style={{ color: 'black', backgroundColor: 'black', height: 0.5 }} />
          {loading ? (
            <Comp.Loading>{loadingText}</Comp.Loading>
          ) : (
            chartData.labels.length > 0 ? (
            <Bar
              data={chartData}
              options={{
                responsive: true,
                plugins: {
                  legend: { position: 'top' },
                  title: { display: true, text: 'Sales Order Chart' }
                }
              }}
            />

            ) : (
              <p>No data to display. Please select a date range and click SEARCH.</p>
            )
          )}
        </div>
        <Comp.AlertPopup />
      </div>
    </>
  );
};

export default SalesChartHomePage;
