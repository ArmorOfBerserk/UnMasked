import cv2
import mediapipe as mp
import socket
import sys

# Configurazione UDP
UDP_IP = "127.0.0.1"
UDP_PORT = 5065
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

mp_face_mesh = mp.solutions.face_mesh
face_mesh = mp_face_mesh.FaceMesh(
    max_num_faces=1,
    refine_landmarks=False, # Non servono iridi per questo
    min_detection_confidence=0.5,
    min_tracking_confidence=0.5
)

cam = cv2.VideoCapture(0)

while True:
    ret, frame = cam.read()
    if not ret: break
    
    # Mirroring (così destra è destra)
    frame = cv2.flip(frame, 1)
    h, w, c = frame.shape
    rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
    results = face_mesh.process(rgb_frame)

    if results.multi_face_landmarks:
        landmarks = results.multi_face_landmarks[0].landmark
        
        # Landmark 1: Punta del naso
        nose = landmarks[1]
        
        # Coordinate normalizzate (0.0 - 1.0)
        # Sottraiamo 0.5 per avere il centro a (0,0)
        # Range risultante: da -0.5 a +0.5
        x_rel = nose.x - 0.5
        y_rel = nose.y - 0.5
        
        # Invio dati: x,y
        data = f"{x_rel:.3f},{y_rel:.3f}"
        sock.sendto(data.encode(), (UDP_IP, UDP_PORT))

        # Visualizzazione Punto (cerchio pieno)
        cx, cy = int(nose.x * w), int(nose.y * h)
        cv2.circle(frame, (cx, cy), 8, (0, 0, 255), -1) 
        
        # Debug testo
        cv2.putText(frame, f"Pos: {data}", (20, 50), 
                    cv2.FONT_HERSHEY_SIMPLEX, 0.8, (0, 255, 0), 2)

    cv2.imshow("Head Position Tracking", frame)
    if cv2.waitKey(1) & 0xFF == 27: # ESC
        break

cam.release()
cv2.destroyAllWindows()