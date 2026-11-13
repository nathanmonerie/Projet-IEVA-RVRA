# Travail effectué :

- Génération de montagne aléatoire à partir de 8 paramètres :
    - Amplitude
    - Frequency
    - WarpAmplitude
    - WarpFrequency
    - Lacunarity
    - Persistance
    - Nombre d'octaves
- Au centre du mesh se trouve des sliders physiques que l'utilisateur peut utilisé pour changer ces parametres en direct.
- De plus, un bouton physique central permet de changer les valeurs aléatoires utilisées dans la génération du terrain.
- Un shader permet de visualiser facilement l'inclinaison de chaque face, avec un dégradé noir-violet-rouge-jaune de vertical à horizontal.

# Améliorations :

- Plus d'optimisation dans le changement de terrains (mise en place de chunk, ou un changement de résolution par rapport a la distance à l'utilisateur)
- Au lieu de commande fixes, des mouvements pour changer les parametres
    - Le terrain autour de l'utilisateur ne doit pas changer (courbe de "gêle" par rapport a la position x,z de l'utilisateur)