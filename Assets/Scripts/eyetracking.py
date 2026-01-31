import cv2
import mediapipe as mp
import socket
import sys

# --- CONFIGURAZIONE ---
UDP_IP = "127.0.0.1"
UDP_PORT = 5065
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

# --- MEDIAPIPE ---
mp_face_mesh = mp.solutions.face_mesh
face_mesh = mp_face_mesh.FaceMesh(
    max_num_faces=1,
    refine_landmarks=False,
    min_detection_confidence=0.5,
    min_tracking_confidence=0.5
)

cam = cv2.VideoCapture(0)

print(f"--- SERVER AVVIATO ---")
print(f"Target IP: {UDP_IP}:{UDP_PORT}")
print(f"La telecamera è attiva in background.")
print(f"Premi CTRL+C nel terminale per fermare lo script.")

try:
    while True:
        ret, frame = cam.read()
        if not ret:
            print("Errore lettura webcam.")
            break
        
        # Mirroring orizzontale (importante per la coerenza destra/sinistra)
        frame = cv2.flip(frame, 1)
        
        # Conversione colore necessaria per MediaPipe
        rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
        
        # Elaborazione (senza disegno)
        results = face_mesh.process(rgb_frame)

        if results.multi_face_landmarks:
            landmarks = results.multi_face_landmarks[0].landmark
            
            # Landmark 1 = Punta del naso
            nose = landmarks[1]
            
            # Calcolo posizione relativa
            x_rel = nose.x - 0.5
            y_rel = nose.y - 0.5
            
            # Invio dati
            data = f"{x_rel:.4f},{y_rel:.4f}"
            sock.sendto(data.encode(), (UDP_IP, UDP_PORT))
            
            # Opzionale: stampa una riga nel terminale per confermare che è vivo
            # sys.stdout.write(f"\rDati inviati: {data}")
            # sys.stdout.flush()

except KeyboardInterrupt:
    print("\nInterruzione rilevata. Chiusura in corso...")

finally:
    cam.release()
    print("Webcam rilasciata. Script terminato.")