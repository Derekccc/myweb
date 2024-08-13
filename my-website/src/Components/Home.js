import React, { useState } from 'react';

const Home = () => {
  const [searchTerm, setSearchTerm] = useState('');

  const handleSearch = (e) => {
    setSearchTerm(e.target.value);
  };

  return (
    <div>
      <h2>* Home Page *</h2>
      <p style={{color: 'red'}}>Opps!* You Reach Here....... Mean: You did not get any permission to access. </p>
      <p style={{color: 'red'}}>(Please grant the permission from the Admin. Thank you.)</p>
      <br></br>
      <input 
        type="text" 
        placeholder="Search..." 
        value={searchTerm} 
        onChange={handleSearch} 
      />
      <p>Searching for: {searchTerm}</p>
    </div>
  );
};

export default Home;
