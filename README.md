# ESAPI Optimization Rings Generator

## Description
This ESAPI script automates the creation of customizable dose containment rings (shells) for VMAT and IMRT optimization. It provides a clean, WPF-based UI where the user can define the start distances and thicknesses for up to three independent rings, ensuring strict control over the dose gradient.

## Clinical Rationale & Advanced Features
Creating manual rings can be tedious and prone to overlapping errors that confuse the optimization engine. This script solves that by implementing advanced Boolean logic:

* **Multi-Target Merging:** Automatically merges multiple selected PTVs into a single `zPTV_Total` structure before generating the rings.
* **The "Onion Effect" (No Overlap):** Prevents ring overlapping. The script automatically subtracts inner rings from outer rings, ensuring each shell represents an exclusive volume.
* **Skin Sparing (Body Retraction):** Automatically crops the rings inside the patient's body surface (`EXTERNAL`) by a user-defined Skin Flash margin to prevent the optimizer from pushing dose into the skin/air.
* **OAR Intersection Handling:** If a ring intersects with a critical Organ at Risk (OAR), the script automatically extracts that overlapping segment into a new sub-structure (e.g., `zR1_in_Rectum`). This allows the dosimetrist to assign different optimization weights to the ring segment that falls inside the OAR.

## Prerequisites
* Varian Eclipse TPS (ESAPI v15.5 or higher).
* An approved Structure Set containing at least one PTV and the `EXTERNAL` body contour.

## How to Use
1. Open a patient and navigate to the **External Beam Planning** workspace.
2. Run the script.
3. In the UI window:
   * Select one or multiple PTVs (Use `Ctrl` to multi-select).
   * Define the Start Distance and Thickness for up to 3 rings (Leave thickness at `0.0` to skip a ring).
   * Define the Skin Retraction Margin (e.g., 0.5 cm).
   * Select the OARs you want to intersect with the rings.
4. Click **Generate Optimization Rings**.

## Disclaimer
This script generates optimization structures only. It does not optimize or calculate dose. It is intended for research and educational purposes. Always review the generated geometry clinically before proceeding with inverse planning.
