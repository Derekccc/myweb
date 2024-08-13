import { Slide, ToastContainer } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";

const AlertPopup = (props) => {
  return (
    <ToastContainer
      position="bottom-right"
      autoClose={5000}
      hideProgressBar={false}
      newestOnTop={false}
      closeOnClick
      theme="colored"
      rtl={false}
      transition={Slide}
      pauseOnFocusLoss
      draggable
      pauseOnHover
    />
  );
};

export default AlertPopup;
