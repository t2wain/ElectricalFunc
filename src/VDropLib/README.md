# Objective

The features implemented in this library is to peform the following tasks:
- Validate that a given circuit did not exceed the ampacity and voltage drop limit
- Select the minimum cable size for a given circuit

# Voltage Drop Table

A Voltage Drop Table is a listing of maximum cable lengths for all combinations of cable spec and load spec from standard libraries of cable and load. The maximum length is based on the voltage drop calculation with a given voltage drop criteria and other design parameters of a given circuit. Since the electrical parameters of the actual cable and load often deviate slightly among different vendors, these parameters from the standard libraries are values taken as the upper limit of deviations (worst case value). The purpose of a Voltage Drop Table is to simplify the calculation of a minimum cable size for a given circuit. Given a load size and cable length of a circuit, a minimum cable size can be looked up from the table. 