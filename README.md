# 🏰 DodgeQuest

Un **bullet hell** con estética **dark fantasy**, donde un héroe solitario se enfrenta a hordas de enemigos y a un poderoso **Boss**.  

Camino: Lunático

Video: 

---

## 🎮 Mecánicas principales

### 🔥 Fases del juego
1. **Phase 1 (0s – 30s)**  
   - Aparecen enemigos comunes en el área definida (`AreaEnemy`).  
   - Estos enemigos disparan balas en distintos **patrones progresivos**.  
   - El jugador gana vida al derrotar enemigos (+2 HP por enemigo destruido).  

2. **Phase 2 (Boss Fight, > 30s)**  
   - Aparece el **Boss**, con vida propia (`maxHP = 200`).  
   - El Boss utiliza patrones de disparo más complejos, cambiando cada **10 segundos**.  
   - El juego termina si:
     - El **Boss muere** (victoria del jugador).  
     - El **jugador muere** (derrota).  

---

### 🗡️ Player
- **HP inicial:** 30 (sin límite máximo).  
- **Movimiento:**  
  - WASD o Flechas direccionales.  
  - Modo lento con `Shift` (reduce velocidad para esquivar mejor).  
- **Disparo:**  
  - Teclas `Z` o `Space`.  
  - Cadencia ajustable (`fireRate`).  
  - Balas rápidas que se destruyen automáticamente tras cierto tiempo o al salir de la pantalla.  
- **Invulnerabilidad breve** después de recibir un golpe (`hitInvuln`).  
- **Curación dinámica:** +2 HP al eliminar un enemigo.  

---

### 👹 Enemigos
- **Vida básica:** 3 HP.  
- **Spawneo automático:** definidos por un rango en `AreaEnemy`.  
- **Patrones de disparo:**  
  1. **Single** → Dispara una bala directa hacia el jugador.  
  2. **Fan** → Tres disparos en abanico dirigidos al jugador.  
  3. **Circle** → Una ráfaga circular de múltiples proyectiles.  
- Los enemigos desaparecen si salen del área de juego o si son destruidos.  

---

### 💀 Boss
- **HP inicial:** 200.  
- **Aparece en Phase 2 (30s).**  
- **No deja de disparar** hasta que el jugador muera o sea derrotado.  
- **Patrones de disparo cíclicos:**  
  1. **Fan** → ráfagas oscilantes en abanico.  
  2. **Circle Burst** → círculos completos de proyectiles.  
  3. **Rose Star** → patrón caótico en forma de estrella/rosa giratoria.  
- **Cambia de patrón cada 10s.**  
- Al morir, el juego finaliza inmediatamente con la **victoria del jugador**.  

---

### 📊 Contadores (HUD)
En pantalla se muestran los siguientes valores en tiempo real:  

- **Active Bullets** → cantidad total de balas activas.  
- **Player Bullets** → disparos creados por el jugador.  
- **Enemy Bullets** → disparos creados por enemigos y boss.  
- **Active Enemies** → número de enemigos activos en el área.  
- **Player HP** → vida actual del jugador.  
- **Boss HP** → vida actual del boss (durante Phase 2).  
- **Time** → cronómetro global del nivel.  

---

### 🕹️ Controles
- **Movimiento:** `WASD` o `Flechas`.  
- **Disparo:** `Z` o `Space`.  
- **Modo Lento (Focus):** `Shift`.  

---

### ⚔️ Condiciones de fin de juego
- **Victoria:** derrotar al Boss antes de los 60 segundos.  
- **Derrota:** el jugador pierde toda su vida.  

---

### ⚡ Aspectos técnicos
- Implementación en **Unity** (C#).  
- Control de tiempo centralizado con `TimeManager`.  
- Gestión de fases y transiciones con `LevelController`.  
- Enemigos controlados mediante `AreaEnemy` (respawn automático).  
- Sistema de eventos para actualizar **HUD** en tiempo real.  
- Balas y entidades con lógica de auto-destrucción fuera del área de juego.  

---

## Turoriales / Refencias
- https://www.youtube.com/watch?v=YNJM7rWbbxY&t=2s
- https://www.youtube.com/watch?v=QQ3Yub9So2k&list=PLfx-IAtqi4dpQ5x3pmuLg5pDGpkTxhniU&index=22&t=184s
- https://docs.unity3d.com/ScriptReference/Quaternion.Euler.html
- https://docs.unity3d.com/ScriptReference/Object.Destroy.html
- https://docs.unity3d.com/ScriptReference/Object.Instantiate.html

## Música
- https://www.youtube.com/watch?v=vI992L2gYPM
- https://artlist.io/royalty-free-music/search?terms=medieval&durationMin=61&includedIds=311&includedIds=6&includedIds=105&durationMax=81

---

🕯️ *Sobrevive al infierno de balas, desafía al Boss y demuestra que puedes resistir en este oscuro universo dark fantasy.*