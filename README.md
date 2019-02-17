# Othello - IA
Cours d'Intelligence Artificielle.

Abdalla Farid, Chacun Guillaume - IA: Jack

2018-2019

## Fonction d'évaluation
Notre fonction d'évaluation se base essentiellement sur des bonus / malus ajoutés à un score calculé en fonction du nombre de pions sur le plateau.

Le score suit la stratégie nommée "Evaporation Strategy" qui consiste à jouer peu de pièces durant la première partie du jeu. Cela a été implémenté en multipliant le nombre de pions du joueur sur le plateau par un ratio. Ce dernier est calculé de la sorte : `Math.Max(EARLY_ROUNDS - currentRoundNumber, 0.0) / EARLY_ROUNDS`. La constante `EARLY_ROUNDS` a pour valeur 25. On transforme donc le score linéairement en fonction du nombre de tours joués. Avant 25 tours de jeu, le score n'est pas complétement pris en compte puis, passé les 25 tours, le nombre de pions sur le plateau est utilisé tel quel (plus modifié).

Les bonus ajoutés à ce score correspondent aux points correspondants aux positions sur le plateau suivant :

```
[ ][0 1 2 3 4 5 6 7 8] 
0   A B C C C C C B A
1   B B D D D D D B B
2   C D           D C
3   C D           D C
4   C D           D C
5   B B D D D D D B B
6   A B C C C C C B A 
```

A : Bonus de 500 points. Les coins permettent de prendre des pièces dans les 3 directions tout en étant imprenables. C'est pourquoi la prises de ces points est priviligiée avec un gros bonus

B : Malus de 80 points. Ces positions simplifient la prise du coin par l'ennemi. Il faut donc éviter de poser une pièce à cet endroit, d'où le malus.
Note: ce malus n'est appliqué que si le coin concerné est libre.

C : Malus de 30 points. Jouer contre les murs nous semblait initialement être une bonne idée mais il s'est avéré que l'IA semble être plus efficace en évitant ces positions. 

D : Malus de 20 points. Permet de privilégier le centre par rapport aux cases du bord.

Les bonus sont simplement sommés au score "de base".

Nous disposons aussi d'un malus appelé frontière. Ce malus correspond au nombre de cases vides autour de la pièce jouée. Il permet de privilégier la pose de pièces dans des cases entourées d'autres pièces évitant ainsi de laisser la possibilité à l'adversaire de retourner notre pièce.

## Particularités de notre implémentation
Le calcul de la "fitness value" est effectué sur chaque noeud de l'arbre (sauf la racine). Ce score est ajouté ou soustrait à celui de ses parents (passé en paramètre de Alphabeta). Il est sommé dans le cas où nous réalisons l'opération et soustrait dans le cas où c'est notre adversaire qui joue. Arrivé à une feuille, on obtient un score représentatif de l'ensemble des actions de la branche (comptées positivement ou négativement en fonction de qui les effectue).

Afin d'éviter de devoir passer des Tuples de Tuples dans les différents paramètres et retours des méthodes d'Alphabeta, nous avons implémenté une classe AlphabetaNode. Cette dernière permet de stocker une opération et le score engendré. Elle propose des accesseurs sur ces paramètres.

## Liens utiles pour les différentes stratégies (consultés le 17.02.19) :
http://mnemstudio.org/game-reversi-example-2.htm

http://samsoft.org.uk/reversi/strategy.htm
