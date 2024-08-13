import LoadingModal from "./LoadingModal";
import { SpinnerDotted } from "spinners-react";

const Loading = (props) => {
  return (
    <LoadingModal onClose={props.onClose}>
      <div
        style={{
          color: "white",
          fontSize: "30px",
          fontFamily: "Calibri, sans-serif",
        }}
      >
        <SpinnerDotted
          size={140}
          thickness={100}
          speed={150}
          color="rgba(15, 116, 219, 1)"
        />
        &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;
        {props.children}
      </div>
    </LoadingModal>
  );
};

export default Loading;
