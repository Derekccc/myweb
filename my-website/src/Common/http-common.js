import axios from "axios";
import { c_API_IP } from "./commonURL";

export default axios.create({
  baseURL: c_API_IP,
  headers: {
    "Content-type": "application/json",
  },
});