
# Recunoașterea obiectelor din imagini în timp real utilizând Microsoft HoloLens 2

## Sumarul analizei literaturii de specialitate


| # | Title | Authors | Domain | Technologies | Methodology | Results | Limitations |
|---|-------|---------|--------|--------------|-------------|---------|-------------|
| 1 | 3D Real-time Face Acupoints Recognition System Based on HoloLens 2 | Xiyu Chen, Hongyu Yang, Yulong Ji, Dongnan Chen | Traditional Chinese Medicine | Microsoft HoloLens2 | <details><summary>View</summary>Face and landmark detection -> 2D acupoints mapped to 3D via depth image -> visualization in HoloLens</details> | Visualization of acupoints | ~2.55mm error, 20.59 FPS |
| 2 | Research on Object Detection based on HoloLens2 | Hao Sua, Zilong Guob, Haotian Luc | Image Recognition | HoloLens2, YOLO-v4 | <details><summary>View</summary>YOLO-v4 applied via HoloLens for fast object detection using its backbone and neck structure</details> | HoloLens-based object detection | Poor for occluded/overlapping/small objects |
| 3 | AR for Phantom Limb Pain Treatment | Cosima Prahm, Michael Bressler, et al. | Medicine | HoloLens2, Mirrors | <details><summary>View</summary>AR-assisted mirror therapy for phantom limb pain, without therapist</details> | Phantom pain disappears | Reliance on HoloLens2 |
| 4 | Object Recognition on HoloLens 2 for Assembly | Ryan George | Industrial Assembly | Azure Custom Vision, HoloLens2 | <details><summary>View</summary>3D models convey assembly steps; parts tracked for virtual-object alignment</details> | >80% match score, some false positives | Limited FoV & display density in HoloLens2 |
| 5 | Small Object Detection in Manual Assembly | Hooman Tavakoli, Snehal Walunj, et al. | Manual Assembly | YOLOv4, HoloLens2 | <details><summary>View</summary>CAD-based training data + YOLOv4 + 2-stage detection (context then object)</details> | 70% mAP at 10% IoU; 9.4 FPS (near real-time) | 9.4 FPS runtime |



# Schema bloc
                    +-----------------------------+
                    |    HoloLens 2 / Emulator    |
                    +-----------------------------+
                    |                             |
                    |      Direcția privirii      |
                    |                             |
                    +-------------+---------------+
                                  |
                                  |  
                                  |
                    +-------------v---------------+
                    |           XR Rig            |
                    +-----------------------------+
                    |                             |
                    |     Poziție + rotație cap   |
                    |       (Main Camera XR)      |
                    |                             |
                    +-------------+---------------+
                                  |
                                  | 
                                  |
                    +-------------v---------------+
                    |      Modul detecție Gaze    |
                    +-----------------------------+
                    |     Raycast din cameră      |
                    |    pe direcția privirii     |
                    |                             |
                    +-------------+---------------+
                                  |
                                  |
                                  |
                    +-------------v---------------+
                    |   Obiect virtual detectat   |
                    +-----------------------------+
                    |         (Cub / Sferă        |
                    |       Cilindru/ Capsulă)    |
                    |                             |
                    +-------------+---------------+
                                  |
                                  |
                                  |
                    +-------------v---------------+
                    |       Afișare pe PC         |
                    +-----------------------------+
                    |     Evidențiere obiect      |
                    |     (schimbare culoare      |
                    |     + denumire obiect)      |
                    +-----------------------------+
