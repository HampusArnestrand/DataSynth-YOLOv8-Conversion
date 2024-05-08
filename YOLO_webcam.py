from ultralytics import YOLO
import cv2
#hello hampus
#hello casper

# start webcam
cap = cv2.VideoCapture(0)
cap.set(3, 640)
cap.set(4, 640)

# model
model = YOLO('./s_ubuntu_640.pt')  # YOLOv8 trained model


while True:
    success, frame = cap.read()
    
    if success:
        # Run YOLOv8 inference on the frame
        results = model(frame,conf=0.4,imgsz=640)

        # Visualize the results on the frame
        annotated_frame = results[0].plot()
        cv2.namedWindow('yolo inference', cv2.WINDOW_NORMAL)
        cv2.setWindowProperty('yolo inference', cv2.WND_PROP_FULLSCREEN, cv2.WINDOW_FULLSCREEN)
        cv2.imshow('yolo inference', annotated_frame)
        if cv2.waitKey(1) == ord('q'):
            break

cap.release()
cv2.destroyAllWindows()