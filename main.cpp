#include <opencv2/opencv.hpp>
#include <iostream>
#include <fstream>

using namespace cv;
using namespace std;
using namespace dnn;

float nmsThreshold = 0.001;         // prag pentru filtrarea nms
float confidenceThreshold = 0.001;  // prag de confidenta
int main() {
    //std::string videoStreamURL = "169.254.129.113/api/holographic/stream/live_high.mp4";

    string modelConfiguration = "yolov3-tiny.cfg";
    string modelWeights = "yolov3-tiny.weights";
    string classesFile = "coco.names";

    // lista de clase
    vector<string> classes;
    ifstream ifs(classesFile.c_str());
    string line;
    while (getline(ifs, line)) classes.push_back(line);

    // incarca reteaua YOLO
    Net net = readNetFromDarknet(modelConfiguration, modelWeights);
    net.setPreferableBackend(DNN_BACKEND_OPENCV);
    net.setPreferableTarget(DNN_TARGET_CPU); 

    string tempVideo = "vidoss.mp4";
    // deschide fluxul video
    cv::VideoCapture cap(tempVideo);
    if (!cap.isOpened()) { 
        std::cerr << "Nu se poate deschide fluxul video!" << std::endl;
        return -1;
    }
    Mat frame, blob;
    while (true) {
        cap >> frame;
        if (frame.empty()) break;
        // preproceseaza cadrul pentru yolo
        blobFromImage(frame, blob, 1 / 255.0, Size(345,345), Scalar(0, 0, 0), true, false);
        net.setInput(blob);
        // obtine predictiile
        vector<Mat> outs;
        net.forward(outs, net.getUnconnectedOutLayersNames());
        vector<int> classIds;
        vector<float> confidences;
        vector<Rect> boxes;

        // postprocesare pentru a extrage obiectele detectate
        for (size_t i = 0; i < outs.size(); ++i) {
            float* data = (float*)outs[i].data;
            for (int j = 0; j < outs[i].rows; ++j, data += outs[i].cols) {
                Mat scores = outs[i].row(j).colRange(5, outs[i].cols);
                Point classIdPoint;
                double confidence;
                minMaxLoc(scores, 0, &confidence, 0, &classIdPoint);

                if (confidence > confidenceThreshold) { // threshold pentru detectie
                    int centerX = (int)(data[0] * frame.cols);
                    int centerY = (int)(data[1] * frame.rows);
                    int width = (int)(data[2] * frame.cols);
                    int height = (int)(data[3] * frame.rows);
                    int left = centerX - width / 2;
                    int top = centerY - height / 2;

                    classIds.push_back(classIdPoint.x);
                    confidences.push_back((float)confidence);
                    boxes.push_back(Rect(left, top, width, height));

                    /*rectangle(frame, Point(left, top), Point(left + width, top + height), Scalar(0, 255, 0), 2);
                    string label = format("%.2f", confidence);
                    label = classes[classIdPoint.x] + ":" + label;
                    putText(frame, label, Point(left, top - 10), FONT_HERSHEY_SIMPLEX, 0.5, Scalar(0, 255, 0), 1);*/
                }
            }
        }
        // aplica filtrarea nms
        vector<int> indices;
        NMSBoxes(boxes, confidences, confidenceThreshold, nmsThreshold, indices);

        for (size_t i = 0; i < indices.size(); ++i) {
            int idx = indices[i];
            Rect box = boxes[idx];
            rectangle(frame, box, Scalar(255, 0, 0), 2);

            string label = format("%.2f", confidences[idx]);
            if (!classes.empty()) {
                label = classes[classIds[idx]] + ": " + label;
            }
            putText(frame, label, Point(box.x, box.y - 10), FONT_HERSHEY_SIMPLEX, 0.5, Scalar(255, 0, 0), 1);
        }
        imshow("Detectie Obiecte", frame);// afiseaza cadrul cu detectiile

        if (waitKey(1) == 'q') break;
    }

    cap.release();
    destroyAllWindows();
    return 0;
}